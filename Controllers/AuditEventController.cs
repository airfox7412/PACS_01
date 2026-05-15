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
/// 系統稽核明細
/// </summary>
[Authorize]
public class AuditEventController : ControllerBase<AuditEventRepository, AuditEvent, AuditEventModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override AuditEventModel ToModel(AuditEvent entity)
    {
        var accRep = new AccountRepository(Repository);
        var model = entity.Adapt<AuditEventModel>();
        var strType = model.Type.ToString();
        model.TypeName = CommonHelper.GetSystemcodeName(47, strType);

        var pracId = model.DataKey;
        model.AccountName = accRep.DataSet.FirstOrDefault(c => c.No == model.AccountNo)?.Name;
        return model;
    }

    ///// <summary>
    ///// Modify
    ///// </summary>
    ///// <param name="Id"></param>
    ///// <returns></returns>
    //[HttpPost("/Audit/Modify")]
    //public ActionResult Modify(DateTime Id)
    //{
    //    if (!CommonHelper.CheckSuper(User.Identity!.Name))
    //        return BadRequest();

    //    var MysqlDB = SystemConfig.MysqlDB;
    //    using var conn = new MySqlConnection(MysqlDB); //dapper 建立連線物件
    //    conn.Open(); //開啟連線
    //    var sSQL = "Delete FROM Audit ";
    //    sSQL += "WHERE CreateTime <= '" + Id.ToString("yyyy-MM-dd 23:59:59") + "' ";
    //    conn.Execute(sSQL); //執行
    //    return Ok();
    //}

    /// <summary>
    /// 稽核建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public AuditEventController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}