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
/// 代碼檔
/// </summary>
[Authorize]
public class ReportCodeDescController : ControllerBase<ReportCodeDescRepository, ReportCodeDesc, ReportCodeDescModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public ReportCodeDescController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}