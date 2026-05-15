using Database.Core;
using Database.Core.Interfaces;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Api.Entities;
using Api.Library;
using Api.Models;
using Api.Repositories;
using NLog;
using Mapster;
using MySql.Data.MySqlClient;
using Dapper;

namespace Api.Controllers;

/// <summary>
/// 報告資料
/// </summary>
[Authorize]
public class ExamDataController : ControllerBase<ExamDataRepository, ExamData, ExamDataModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override ExamDataModel ToModel(ExamData entity)
    {
        var accRep = new AccountRepository(Repository);
        var model = entity.Adapt<ExamDataModel>();

        return model;
    }

    /// <summary>
    /// UpdateExamData
    /// </summary>
    /// <returns></returns>
    [HttpGet("/ExamData/UpdateExamData")]
#if DEBUG
    [AllowAnonymous]
#endif
    public IActionResult UpdateExamData()
    {
        var sdate = DateTime.Parse("2025-05-13");
        var edate = DateTime.Parse("2025-06-05");
        var eds = Repository.DataSet
            .Where(c => c.ExamDateTime >= sdate.Date && c.ExamDateTime <= edate.Date)
            .ToList();
        foreach (var item in eds)
        {
            if(!item.Memo1.Contains("@")) continue;
            var arr = item.Memo1.Split('@');
            item.Recommand = arr[1];
            Repository.Update(item);
        }
        return Ok();
    }

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public ExamDataController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}