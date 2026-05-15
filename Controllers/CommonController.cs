using Microsoft.AspNetCore.Mvc;
using Api.Library;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Syncfusion.XlsIO;
using System;
using System.IO;
using System.Linq;

namespace Api.Controllers;

/// <summary>
/// 共用API
/// </summary>
public class CommonController : ControllerBase
{
    /// <summary>
    /// 身分證檢查
    /// </summary>
    /// <param name="identity">身分證號</param>
    /// <returns></returns>
    [HttpGet]
    [Route("/Common/CheckTWId")]
    public IActionResult CheckTWId(string identity)
    {
        var result = CommonHelper.CheckTWId(identity);
        return Ok(result);
    }

    /// <summary>
    /// 居留證檢查
    /// </summary>
    /// <param name="identity">居留證號</param>
    /// <returns></returns>
    [HttpGet]
    [Route("/Common/CheckResidentID")]
    public IActionResult CheckResidentID(string identity)
    {
        var result = CommonHelper.CheckResidentID(identity);
        return Ok(result);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("/Common/ChangeStudyId")]
#if DEBUG
    [AllowAnonymous]
#endif
    public IActionResult ChangeStudyId()
    {
        var dpRep = new DicomPictureRepository();
        var dps = dpRep.DataSet.Where(c => c.StudyID.Contains("LD")).ToList();
        foreach (var item in dps)
        {
            var image = item.Image;
            var fname = Path.GetFileName(image);
            var studyId = fname[..^6];
            item.StudyID = studyId;
            dpRep.Update(item); System.Diagnostics.Debug.WriteLine($"StudyID: {studyId}");
        }
        return Ok();
    }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Common/GetExcel")]
#if DEBUG
    [AllowAnonymous]
#endif
    public IActionResult GetExcel()
    {
        var pdRep = new PatientDetailRepository();
        var edRep = new ExamDataRepository();
        var dpRep = new DicomPictureRepository();

        var excelFile = "D:\\AccessionNum_StudyId20250527.xlsx";
        using (FileStream stream = new FileStream(excelFile, FileMode.Open, FileAccess.Read))
        {
            using (var excelEngine = new ExcelEngine())
            {
                IApplication app = excelEngine.Excel;
                app.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = app.Workbooks.Open(stream);
                IWorksheet worksheet = workbook.Worksheets[0];
                int rowCount = worksheet.UsedRange.LastRow;
                for (int i = 2; i <= rowCount; i++) // 假設第一行是標題
                {
                    string ano = worksheet[$"A{i}"].Text;
                    string cno = worksheet[$"B{i}"].Text;
                    var pd = pdRep.DataSet.FirstOrDefault(c => c.AccessionNumber == ano);
                    if (pd != null)
                    {
                        pd.StudyID = cno;
                        pdRep.Update(pd);
                        System.Diagnostics.Debug.WriteLine($"AccessionNo: {ano}, CaseNo: {cno}");
                    }
                    var ed = edRep.DataSet.FirstOrDefault(c => c.StudyID == ano);
                    if (ed != null)
                    {
                        ed.StudyID = cno;
                        edRep.Update(ed);
                        System.Diagnostics.Debug.WriteLine($"AccessionNo: {ano}, CaseNo: {cno}");
                    }
                    //var dp = dpRep.DataSet.Where(c => c.StudyID == ano).ToList();
                    //foreach( var ditem in dp )
                    //{
                    //    ditem.StudyID = cno;
                    //    dpRep.Update(ditem);
                    //    System.Diagnostics.Debug.WriteLine($"AccessionNo: {ano}, CaseNo: {cno}");
                    //}
                }
            }
        }
        return Ok();
    }
}