using Database.Core;
using Database.Core.Interfaces;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using Api.Entities;
using Api.Models;
using Api.Repositories;
using NLog;
using Mapster;
using Api.Library;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Api.Controllers;

/// <summary>
/// 病患檢查明細檔
/// </summary>
[Authorize]
public class PatientDetailController : ModelControllerBase<PatientDetailRepository, PatientDetail, PatientDetailModel, PatientDetailModel, PatientDetailModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    //獲取應用程式所在目錄（絕對，不受工作目錄影響，建議採用此方法獲取路徑)
    private static string basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override PatientDetailModel ToModel(PatientDetail entity)
    {
        var model = entity.Adapt<PatientDetailModel>();
        return model;
    }

    /// <summary>
    /// 新增檢查單號
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost("/PatientDetail/PostExamine")]
    public async Task<IActionResult> PostExamine([FromForm] ExamineModel body)
    {
        try
        {
            var pd = Repository.DataSet.FirstOrDefault(c => c.AccessionNumber == body.AccessionNo);
            if (pd == null)
            {
                var model = new PatientDetail
                {
                    StudyID = body.StudyID,
                    AccessionNumber = body.AccessionNo,
                    InstanceNumber = body.InstanceNumber,
                    PatientID = body.PatientID,
                    Modality = body.Modality,
                    BodyPartExamined = body.BodyPartExamined,
                    StudyDate = body.StudyDateTime.Date,
                    StudyTime = body.StudyDateTime.TimeOfDay,
                    Status = "N"
                };
                Repository.Insert(model);
            }
            else
            {
                pd.StudyID = body.StudyID;
                pd.AccessionNumber = body.AccessionNo;
                pd.InstanceNumber = body.InstanceNumber;
                pd.PatientID = body.PatientID;
                pd.Modality = body.Modality;
                pd.BodyPartExamined = body.BodyPartExamined;
                pd.StudyDate = body.StudyDateTime.Date;
                pd.StudyTime = body.StudyDateTime.TimeOfDay;
                Repository.Update(pd);
            }

            var edRep = new ExamDataRepository(Repository);
            var edData = edRep.DataSet.FirstOrDefault(c => c.StudyID == body.AccessionNo);
            if (edData == null)
            {
                var edModel = new ExamData
                {
                    StudyID = body.AccessionNo
                };
                edRep.Insert(edModel);
            }
            var dpRep = new DicomPictureRepository();
            if (body.Files == null || body.Files.Count == 0)
            {
                return BadRequest("請上傳至少一個檔案");
            }

            foreach (var file in body.Files)
            {
                var filePath = Path.Combine(SystemConfig.UploadFiles, body.PatientID);
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                var jpgFile = Path.GetFileName(file.FileName);
                var filename = Path.Combine(filePath, jpgFile);
                using var stream = new FileStream(filename, FileMode.Create);
                await file.CopyToAsync(stream);
                var image = $"\\{body.PatientID}\\{jpgFile}";
                var pmodel = new DicomPicture
                {
                    StudyID = body.AccessionNo,
                    Image = image
                };

                var dp = dpRep.DataSet.FirstOrDefault(c => c.StudyID == pmodel.StudyID && c.Image == pmodel.Image);
                if (dp == null)
                    await dpRep.InsertAsync(pmodel);
            }
            return Ok("儲存成功");
        }
        catch (Exception ex)
        {
            logger.Error($"PostExamine->{ex.Message}");
            return BadRequest("儲存失敗");
        }
    }

    /// <summary>
    /// ToReadModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private static ExamDataReadModel ToReadModel(PatientDetail entity)
    {
        var pdRep = new PatientDetailRepository();
        var edRep = new ExamDataRepository(pdRep);
        var edData = edRep.DataSet.FirstOrDefault(c => c.StudyID == entity.AccessionNumber);
        if (edData == null) return null;

        var chnReport = edData.Memo1;
        var engReport = edData.English;
        var RecommendReport = edData.Recommand;
        var Report_Updata_Tm = edData.ExamDateTime?.ToString("yyyy-MM-dd");
        var Report_Finish_Tm = edData.ExamDateTime?.ToString("yyyy-MM-dd HH:mm:ss");

        var model = new ExamDataReadModel
        {
            Accession_num = entity.AccessionNumber,
            Patient_ID = entity.PatientID,
            Lines = "1",
            Text_Type = "1", //1診斷,2建議
            Text_CH = chnReport,
            Text_EN = engReport,
            Report_Updata_Tm = Report_Updata_Tm,
            ScheduledPerformingPhysician = "林士超", //醫師姓名
            verification_physician = "scott", //醫師帳號
            Report_Finish_Tm = Report_Finish_Tm,
            RecommendReport = RecommendReport
        };
        return model;
    }

    /// <summary>
    /// 取得報告資料
    /// </summary>
    /// <param name="opts"></param>
    /// <returns></returns>
    [HttpGet("/PatientDetail/ExamReport")]
#if DEBUG
    [AllowAnonymous]
#endif
    public ActionResult<ODataPageResult<ExamDataReadModel>> ExamReport(ODataQueryOptions<PatientDetail> opts)
    {
        var datas = Repository.DataSet.Where(c => c.Status == "Y");
        var pageModels = Repository.GetModelPage(opts, datas, ToReadModel);
        return pageModels;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("/PatientDetail/ChangeImage")]
    public ActionResult ChangeImage()
    {
        var imgPath = SystemConfig.UploadFiles;
        var dpRep = new DicomPictureRepository(Repository);
        var dpDatas = dpRep.DataSet.ToList();
        foreach (var data in dpDatas)
        {
            var pdData = Repository.DataSet.FirstOrDefault(c => c.StudyID == data.StudyID);
            var patid = pdData.PatientID;
            var anoid = data.Image;
            //var anoPath = imgPath.Replace("UploadFiles\\", "UploadFiles") + anoid;
            var anoPath = imgPath + anoid;
            var imgName = Path.GetFileName(anoPath);
            var patPath = Path.Combine(imgPath, patid);
            if (!Directory.Exists(patPath))
                Directory.CreateDirectory(patPath);
            var patPathName = $"{patPath}\\{imgName}";
            if (!System.IO.File.Exists(patPathName))
            {
                System.IO.File.Copy(anoPath, patPathName);
            }
            System.IO.File.Delete(anoPath);
            System.Diagnostics.Debug.WriteLine($"{anoPath} => {patPathName}");
            var image = $"\\{patid}\\{imgName}";
            data.Image = image;
            dpRep.Update(data);
        }
        return Ok();
    }

    /// <summary>
    /// ReGetHistory
    /// </summary>
    /// <returns></returns>
    [HttpGet("/PatientDetail/ReGetHistory")]
#if DEBUG
    [AllowAnonymous]
#endif
    public IActionResult ReGetHistory()
    {
        var sdate = DateTime.Parse("2025-05-13");
        var pdGroups = Repository.DataSet
            .Where(c => c.StudyDate < sdate.Date)
            .GroupBy(c => c.PatientID);
        var pmRep = new PatientMainRepository();
        foreach (var group in pdGroups)
        {
            var pid = group.Key;
            System.Diagnostics.Debug.WriteLine(group.Key);
            var pm = pmRep.DataSet.FirstOrDefault(c => c.PatientID == pid);
            pm.OtherPatientID = "N";
            pmRep.Update(pm);
        }
        return Ok();
    }

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public PatientDetailController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}