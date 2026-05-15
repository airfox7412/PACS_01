using Api.Entities;
using Api.Library;
using Api.Models;
using Api.Repositories;
using NLog;
using Quartz;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Api.Jobs;

/// <summary>
/// 取得歷史資料
/// </summary>
public class GetHistoryJob : IJob
{
    private static Logger logger => LogManager.GetCurrentClassLogger();
    private static string HistoryUrl = SystemConfig.HistoryUrl;

    /// <summary>
    /// 取得病患資料及影像
    /// </summary>
    /// <param name="Id">身分證號</param>
    /// <returns></returns>
    private static async Task<HistoryListModel> GetHistory(string Id)
    {
        try
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(HistoryUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/text"));
            var apiUrl = string.Format("API/Mgt/GetHistoryPacsReport?patientId=" + Id);
            var response = await client.GetAsync(apiUrl);
            //logger.Debug(response);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                //logger.Debug(responseContent);
                var result = await response.Content.ReadAsAsync<HistoryListModel>();
                return result;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 檢查是否變更狀態
    /// </summary>
    /// <param name="context"></param>
    public async Task Execute(IJobExecutionContext context)
    {
        var pdRep = new PatientDetailRepository();
        var edRep = new ExamDataRepository();
        var pmRep = new PatientMainRepository();
        var dpRep = new DicomPictureRepository();

        #region 處理歷史病例報告
        try
        {
            var pmDatas = pmRep.DataSet.Where(c => c.OtherPatientID != "Y").ToList();
            foreach (var pmItem in pmDatas)
            {
                var model = await GetHistory(pmItem.PatientID);
                if (model == null) continue;

                if (model.Count > 0)
                {
                    foreach (var item in model.Items)
                    {
                        //檢查明細檔
                        try
                        {
                            var pd = pdRep.DataSet.FirstOrDefault(c => c.AccessionNumber == item.AccessionNum);
                            if (pd == null)
                            {
                                //logger.Info($"有病患資料=>{pmItem.PatientID}");
                                var pdmodel = new PatientDetail
                                {
                                    StudyID = item.AccessionNum,
                                    AccessionNumber = item.AccessionNum,
                                    PatientID = item.PatientID,
                                    SeriesNumber = 0,
                                    StudyDate = item.StudyDateTime.Date,
                                    StudyTime = item.StudyDateTime.TimeOfDay,
                                    ContentDate = item.ReportUpdataTm.Date,
                                    ContentTime = item.ReportUpdataTm.TimeOfDay,
                                    Modality = item.Modality,
                                    BodyPartExamined = item.ItemNm,
                                    Status = "Y"
                                };
                                await pdRep.InsertAsync(pdmodel);
                                logger.Info($"增加歷史檢查項目:{item.PatientID}");
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        //報告檔
                        try
                        {
                            var edData = edRep.DataSet.FirstOrDefault(c => c.StudyID == item.AccessionNum);
                            if (edData == null)
                            {
                                var edmodel = new ExamData
                                {
                                    StudyID = item.AccessionNum,
                                    ExamDateTime = item.ReportFinishTm,
                                    Memo1 = item.TextCH,
                                    English = item.TextEN,
                                    Recommand = item.RecommendReport
                                };
                                await edRep.InsertAsync(edmodel);
                                //logger.Info($"增加歷史報告:{pmItem.PatientID}");
                            }
                            else
                            {
                                if (edData.Memo1 == null)
                                {
                                    edData.Memo1 = item.TextCH;
                                    edData.English = item.TextEN;
                                    edData.Recommand = item.RecommendReport;
                                    await edRep.UpdateAsync(edData);
                                    logger.Info($"更新歷史報告:{pmItem.PatientID}");
                                }
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        foreach (var image in item.Thumbnails)
                        {
                            var imgUrl = image
                                .Replace("http://hms.leaderclinic.com.tw/API/Mgt/GetThumbnails?1=1&fileName=", "")
                                .Trim();
                            var dp = dpRep.DataSet.FirstOrDefault(c => c.Image == imgUrl);
                            if (dp == null)
                            {
                                var dpdata = new DicomPicture
                                {
                                    StudyID = item.AccessionNum,
                                    Image = imgUrl
                                };
                                await dpRep.InsertAsync(dpdata);
                                //logger.Info($"新增圖片=>{dpdata.Image}");
                            }
                        }
                    }

                    pmItem.OtherPatientID = "Y";
                    await pmRep.UpdateAsync(pmItem);
                }
            }

            logger.Info("檢查結束");
        }
        catch (Exception ex)
        {
            logger.Error($"檢查歷史資料錯誤: {ex}");
        }
        #endregion
    }
}