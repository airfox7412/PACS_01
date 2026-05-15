using Database.Core;
using Database.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Api.Entities;
using Api.Models;
using Api.Repositories;
using NLog;

namespace Api.Controllers;

/// <summary>
/// 特殊代碼檔
/// </summary>
[Authorize]
public class ReportSpecialWordController : ControllerBase<ReportSpecialWordRepository, ReportSpecialWord, ReportSpecialWordModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public ReportSpecialWordController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}