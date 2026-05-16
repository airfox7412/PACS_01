using Api.Library;
using Api.Repositories;
using NLog;
using Quartz;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Jobs;

/// <summary>
/// 取得Gemini AI OT Value
/// </summary>
public class GetGeminiAIJob : IJob
{
    private static Logger logger => LogManager.GetCurrentClassLogger();
    private readonly IBoneDensityAnalyzer _analyzer;

    // 透過建構子注入 IBoneDensityAnalyzer (此時它已經帶有 HttpClient 和 Polly 策略)
    public GetGeminiAIJob(IBoneDensityAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    /// <summary>
    /// 檢查OT檢查並用AI辨識數值
    /// </summary>
    /// <param name="context"></param>
    public async Task Execute(IJobExecutionContext context)
    {
        var pdRep = new PatientDetailRepository();
        var edRep = new ExamDataRepository();
        var pmRep = new PatientMainRepository();
        var dpRep = new DicomPictureRepository();
        #region AI處理
        logger.Info("開始用AI處理Modality=OT");
        var UploadFilePath = SystemConfig.UploadFiles;
        var pds = pdRep.DataSet.Where(c => c.Modality == "OT" && c.Status == "N").ToList();
        var stopFlag = false;
        foreach (var item in pds)
        {
            if (stopFlag) break; // 若 API 忙碌則停止本次 Batch
            try
            {
                var ed = edRep.DataSet.First(c => c.StudyID == item.AccessionNumber);
                if (string.IsNullOrEmpty(ed.Memo1))
                {
                    var dpMemo1 = ""; // 每個病人開始前清空一次
                    var dps = dpRep.DataSet.Where(c => c.StudyID == item.AccessionNumber).ToList();
                    foreach (var dp in dps) // 處理該病人的多張影像
                    {
                        var examMemo = "";
                        var analyze = "";
                        var jpgFile = dp.Image;
                        var imagePath = $"{UploadFilePath}{jpgFile}"; // 影像路徑檔名
                        //用AI處理影像 
                        var aiResponse = await _analyzer.AnalyzeReportAsync(imagePath);
                        if (aiResponse is "失敗: ServiceUnavailable" or "失敗: TooManyRequests")
                        {
                            stopFlag = true;
                            break;
                        }
                        
                        // 使用正規表示式尋找 負號(可選) + 數字 + 小數點 + 數字
                        var match = Regex.Match(aiResponse, @"-?\d+(\.\d+)?");
                        if (match.Success)
                        {
                            try
                            {
                                // 判斷部位
                                var examPart = aiResponse.Contains("Total") ? "腰椎" : "左髖關節";
                                //轉換成數值
                                var tScore = double.Parse(match.Value);
                                System.Diagnostics.Debug.WriteLine($"提取到的數值為: {tScore}");
                                analyze = tScore switch
                                {
                                    // 進行醫學邏輯判斷
                                    <= -2.5 => "骨質疏鬆(Osteoporosis)",
                                    <= -1.0 => "骨質缺乏(Osteopenia)",
                                    _ => "骨質密度正常"
                                };
                                dpMemo1 += $"{examPart}，T值為 {tScore} ，{analyze}\r\n";
                            }
                            catch
                            {
                                logger.Error($"數值轉換失敗，原始文字: {aiResponse}");
                            }
                        }
                        else
                        {
                            // 屬於正常臨床狀況（如 AI 回傳 Data Not Found），記錄為 Debug/Info 即可，不污染 Error Log
                            logger.Info($"AccessionNumber = {item.AccessionNumber}"); 
                            logger.Info($"影像未含有可解析之 T-Score 數據。AI 回傳: {aiResponse}");
                        }
                    }

                    if (string.IsNullOrEmpty(dpMemo1)) continue;
                        
                    ed.Memo1 = dpMemo1; //解析結果
                    ed.Recommand = SystemConfig.OT_Recommand; //填入參考建議
                    await edRep.UpdateAsync(ed);
                    item.Status = "Y";
                    await pdRep.UpdateAsync(item);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"AI處理錯誤: {ex}");
            }
        }

        logger.Info("結束用AI處理Modality=OT");
        #endregion
    }
}