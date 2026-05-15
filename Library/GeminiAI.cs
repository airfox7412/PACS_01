using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Api.Models;
using Newtonsoft.Json;

namespace Api.Library
{
    /// <summary>
    /// GeminiAI Call
    /// </summary>
    public class GeminiAI
    {

        // 改為從 SystemConfig 讀取
        private static string _apiKey => SystemConfig.GeminiApiKey;
        private static string _endpoint => SystemConfig.GeminiEndpointUrl;

        /// <summary>
        /// 分析報告內容
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static async Task<string> AnalyzeBoneDensityReport(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string base64Image = Convert.ToBase64String(imageBytes);

            using var client = new HttpClient();

            // 1. 定義更精確的指令
            string systemPrompt = @"
        你是一個專業的醫學數據提取助手。
        請分析這張骨質密度(BMD)報告，並嚴格遵守以下規則：
        1. 如果報告中有 'Neck' (股骨頸) 的數據，僅回傳：'Neck T-Score: [數值]'。
        2. 如果報告中有 'Total' (總體) 的數據，僅回傳：'Total T-Score: [數值]'。
        3. 若兩者都有，優先回傳 Neck。
        4. 禁止包含任何解釋、問候或其他文字，只需要輸出要求的字串。
        5. 如果找不到數據，請回傳 'Data Not Found'。";

            // 2. 更新 API 請求內容
            var requestBody = new
            {
                contents = new[]
                {
                        new {
                            parts = new object[]
                            {
                                new { text = systemPrompt }, // 傳入規則
                                new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                            }
                        }
                    }
            };

            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            int maxRetries = 3; // 最多重試 3 次
            int delayMilliseconds = 2000; // 初始等待 2 秒

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await client.PostAsync($"{_endpoint}?key={_apiKey}", content);
                    string result = await response.Content.ReadAsStringAsync();

                    // 如果成功，就解析並回傳
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<GeminiResponseModel>(result);
                        if (jsonResponse?.Candidates != null && jsonResponse.Candidates.Length > 0)
                        {
                            string aiResponse = jsonResponse.Candidates[0].Content.Parts[0].Text;
                            // 使用正規表示式尋找 負號(可選) + 數字 + 小數點 + 數字
                            var match = Regex.Match(aiResponse, @"-?\d+(\.\d+)?");
                            if (match.Success)
                            {
                                return aiResponse;
                            }
                        }

                        return "API 回傳成功，但沒有分析內容。";
                    }

                    // 檢查是否為 503 伺服器忙碌 或 429 請求過多
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine($"[警告] 伺服器忙碌中 ({response.StatusCode})。等待 {delayMilliseconds / 1000.0} 秒後進行第 {i + 1} 次重試...");
                        await Task.Delay(delayMilliseconds);

                        // 指數退避：下次等待的時間翻倍 (2秒 -> 4秒 -> 8秒)
                        delayMilliseconds *= 2;
                        continue; // 繼續下一次迴圈
                    }

                    // 如果是其他錯誤 (例如 400 參數錯誤, 403 權限錯誤)，就直接報錯不重試
                    return $"API 請求失敗，狀態碼: {response.StatusCode}，錯誤內容: {result}";
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"[警告] 網路連線異常: {ex.Message}。等待 {delayMilliseconds / 1000.0} 秒後重試...");
                    await Task.Delay(delayMilliseconds);
                    delayMilliseconds *= 2;
                }
            }

            return "API 請求失敗：已達到最大重試次數，伺服器依然無法回應，請稍後再試。";
        }
    }
}
