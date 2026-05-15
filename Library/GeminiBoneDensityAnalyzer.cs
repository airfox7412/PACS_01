using Api.Models; // 確保引用了 GeminiResponseModel 所在的命名空間
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Api.Library
{
    public class GeminiBoneDensityAnalyzer : IBoneDensityAnalyzer
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        public GeminiBoneDensityAnalyzer(HttpClient httpClient, IOptions<GeminiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> AnalyzeReportAsync(string imagePath)
        {
            if (!File.Exists(imagePath)) return "檔案不存在";

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string base64Image = Convert.ToBase64String(imageBytes);

            // 1. 參考 GeminiAI.cs 的完整 System Prompt
            string systemPrompt = @"
                你是一個專業的醫學數據提取助手。
                請分析這張骨質密度(BMD)報告，並嚴格遵守以下規則：
                1. 如果報告中有 'Neck' (股骨頸) 的數據，僅回傳：'Neck T-Score: [數值]'。
                2. 如果報告中有 'Total' (總體) 的數據，僅回傳：'Total T-Score: [數值]'。
                3. 若兩者都有，優先回傳 Neck。
                4. 禁止包含任何解釋、問候或其他文字，只需要輸出要求的字串。
                5. 如果找不到數據，請回傳 'Data Not Found'。";

            // 2. 構建請求內容
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new object[] {
                            new { text = systemPrompt },
                            new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                        }
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // 注意：Polly 應在此處透過 HttpClient 處理 429 或 503 的重試
                var response = await _httpClient.PostAsync($"?key={_settings.ApiKey}", content);

                string result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    // 針對 Job 層級的 stopFlag 判斷，回傳特定格式
                    return $"失敗: {response.StatusCode}";
                }

                // 3. 解析 JSON，提取 AI 的純文字內容
                var jsonResponse = JsonConvert.DeserializeObject<GeminiResponseModel>(result);
                logger.Debug($"解析JSON => {jsonResponse}");
                if (jsonResponse?.Candidates != null && jsonResponse.Candidates.Length > 0)
                {
                    string aiResponse = jsonResponse.Candidates[0].Content.Parts[0].Text;
                    return aiResponse.Trim(); // 回傳如 "Neck T-Score: -2.5"
                }

                return "Data Not Found";
            }
            catch (Exception ex)
            {
                return $"錯誤: {ex.Message}";
            }
        }
    }
}