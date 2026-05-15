using Database.Core;
using Database.Core.Models;
using Database.Core.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Entities;
using Api.Models;
using Api.Repositories;
using Api.Library;
using Dapper;
using MySql.Data.MySqlClient;
using NLog;

namespace Api.Controllers;

/// <summary>
/// 公用資料
/// </summary>
[Authorize]
public class SystemcodeController : ControllerBase<SystemcodeRepository, Systemcode, SystemcodeModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 0性別
    /// </summary>
    /// <returns></returns>
    [DisplayName("0性別")]
    [HttpGet("/Systemcode/Gender")]
    public IEnumerable<SelectNoModel>Gender() => Repository.GetTexts(0);

    /// <summary>
    /// 建議科別
    /// </summary>
    /// <returns></returns>
    [HttpGet("/Systemcode/Specialty")]
    public IEnumerable<SelectNoModel> Specialty()
    {
        var Specialtys = new List<SelectNoModel>
        {
            new() {No = "AC", Name = "胸腔內科"},
            new() {No = "BC", Name = "胸腔外科"},
            new() {No = "AB", Name = "心臟內科"},
            new() {No = "BB", Name = "心臟外科"},
            new() {No = "22", Name = "急診醫學科"},
            new() {No = "09", Name = "耳鼻喉科"},
            new() {No = "AA", Name = "消化內科"},
            new() {No = "BD", Name = "消化外科"},
            new() {No = "02", Name = "一般內科"},
            new() {No = "03", Name = "一般外科"},
            new() {No = "08", Name = "泌尿外科"},
            new() {No = "05", Name = "婦產科"},
            new() {No = "30", Name = "高壓氧科"},
            new() {No = "AF", Name = "血液腫瘤科"},
            new() {No = "AD", Name = "腎臟內科"},
            new() {No = "12", Name = "神經內科"},
            new() {No = "07", Name = "神經外科"},
            new() {No = "10", Name = "眼科"},
            new() {No = "04", Name = "小兒科"},
            new() {No = "06", Name = "骨科"},
            new() {No = "15", Name = "整形外科"},
            new() {No = "99", Name = "其他"}
        };
        return Specialtys;
    }

    /// <summary>
    /// 應用程式集區applicationPools
    /// </summary>
    /// <returns></returns>
    [HttpGet("/Systemcode/PoolReset")]
    public IActionResult PoolReset()
    {
        if (!CommonHelper.CheckSuper(User.Identity!.Name))
            return Unauthorized();

        const string appcmd = @"%systemroot%\system32\inetsrv\appcmd.exe";
        var result = ExecuteCommand($"{appcmd} list APPPOOL");
        if (!string.IsNullOrEmpty(result.error)) return BadRequest();

        var arrayOutput = result.output.Split('\n');
        foreach (var s in arrayOutput)
        {
            if (string.IsNullOrEmpty(s)) continue;
            var apoolName = ParserPool(s);
            //System.Diagnostics.Debug.WriteLine(apoolName);
            result = ExecuteCommand($"{appcmd} stop APPPOOL \"{apoolName}\"");
            logger.Debug(result.output);
            result = ExecuteCommand($"{appcmd} start APPPOOL \"{apoolName}\"");
            logger.Debug(result.output);
        }
        return Ok();
    }

    private string ParserPool(string s)
    {
        var sInt = 0;
        var eInt = 0;
        for (var x = 0; x < s.Length - 1; x++)
        {
            var w = s.Substring(x, 1);
            if (w == "\"" && sInt == 0)
                sInt = x + 1;
            else if (w == "\"")
            {
                eInt = x - 1;
                break;
            }
        }

        return s.Substring(sInt, eInt - sInt + 1);
    }
    private static outputModel ExecuteCommand(string command)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/c " + command;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Verb = "runas";
        process.Start();
        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        var model = new outputModel
        {
            output = output,
            error = error
        };
        return model;
    }
    private class outputModel
    {
        public string output { get; set; }
        public string error { get; set; }
    }

    private class ThreadModel
    {
        public string Variable_name { get; set; }
        public string Value { get; set; }
    }

    ///// <summary>
    ///// Db Pool Size
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet("/Systemcode/DbPoolValue")]
    //public ActionResult DbPoolValue()
    //{
    //    if (!CommonHelper.CheckSuper(User.Identity!.Name))
    //        return BadRequest();

    //    var MysqlDB = SystemConfig.MysqlDB;
    //    using var conn = new MySqlConnection(MysqlDB); //dapper 建立連線物件
    //    conn.Open(); //開啟連線
    //    var result = conn.Query<ThreadModel>("SHOW STATUS LIKE 'Threads_connected';"); //執行
    //    var count = result.First().Value;
    //    conn.Close();
    //    return Ok($"目前連接到 MySQL 伺服器的活躍連接數: {count}");
    //}

    ///// <summary>
    ///// Db Pool Clear
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet("/Systemcode/DbPoolClear")]
    //public ActionResult DbPoolClear()
    //{
    //    if (!CommonHelper.CheckSuper(User.Identity!.Name))
    //        return BadRequest();

    //    var MysqlDB = SystemConfig.MysqlDB;
    //    using var conn = new MySqlConnection(MysqlDB); //dapper 建立連線物件
    //    conn.Open(); //開啟連線
    //    MySqlConnection.ClearAllPools(); //清除所有連接池
    //    return Ok();
    //}

    /// <summary>
    /// 稽核
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public SystemcodeController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}