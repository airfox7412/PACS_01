using Database.Core;
using Database.Core.Enums;
using Database.Core.Interfaces;
using Database.Core.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Api.Entities;
using Api.Enums;
using Api.Models;
using Api.Repositories;
using Api.Library;
using Syncfusion.XlsIO;
using Mapster;
using NLog;
using Newtonsoft.Json;

namespace Api.Controllers;

/// <summary>
/// 使用者帳號API
/// </summary>
[Authorize]
public class AccountController : ModelControllerBase<AccountRepository, Account, AccountModel, AccNewModel, AccUpdateModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private static string basePath = AppContext.BaseDirectory;

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override AccountModel ToModel(Account entity)
    {
        var model = entity.Adapt<AccountModel>();
        try
        {
            var statusName = model.Status switch
            {
                100 => "啟用",
                0 => "停用",
                1 => "暫停",
                _ => ""
            };
            model.StatusName = statusName;
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }

        return model;
    }

    /// <summary>
    /// 取得單一使用者Model
    /// </summary>
    /// <param name="No"></param>
    [HttpGet("/Account/GetModel")]
    public AccReadModel GetModel(string No)
    {
        var acc = Repository.DataSet.FirstOrDefault(c => c.No == No);
        if (acc == null) return null;
        var Identifier = "";
        DateTime? BirthDate = null;
        var Weight = "";
        var GenderCode = "";
        var Address = "";                
        var model = new AccReadModel
        {
            No = acc.No,
            Name = acc.Name,
            Phone = acc.Phone,
            Email = acc.Email,
            Identifier = Identifier,
            Expiration = acc.Expiration,
            Weight = Weight,
            GenderCode = GenderCode,
            Address = Address,
            Status = acc.Status
        };
        if (BirthDate != null)
            model.BirthDate = BirthDate;
        return model;
    }

    /// <summary>
    /// 新增使用者
    /// </summary>
    /// <param name="body"></param>
    public override async Task<IActionResult> Post([FromBody] AccNewModel body)
    {
        var jsonString = JsonConvert.SerializeObject(body, Formatting.Indented);
        logger.Info($"AccNewModel=>{jsonString}");

        if (!CreateValidation(body))
            return BadRequest(ValidationMessage);

        var userNo = User.Identity!.Name;
        var account = new Account
        {
            No = body.No,
            Name = body.Name,
            Identifier = body.Identifier,
            Phone = body.Phone,
            Email = body.Email,
            Expiration = body.Expiration,
            Status = body.Status,
            IsSuper = false,
            LastLoginTime = null
        };

        try
        {
            var insertAccount = Repository.InsertAsync(account);
            var AuditEvent =  AuditEventService.Add(AuditEventType.Insert, account, TokenManager.Operationer(User.Identity!.Name));
            await Task.WhenAll(insertAccount, AuditEvent);
            return Ok("新增成功");
        }
        catch (Exception ex)
        {
            await AuditEventService.Add(AuditEventType.Insert, ex.InnerException, TokenManager.Operationer(User.Identity!.Name));
            return BadRequest("新增帳號失敗");
        }
    }

    /// <summary>
    /// 修改使用者
    /// </summary>
    /// <param name="body"></param>
    [DisplayName("修改使用者")]
    public override async Task<IActionResult> Put([FromBody] AccUpdateModel body)
    {
        if (!body.AccountSubInstitutions.Any())
            return BadRequest("服務機構未選擇");

        var userNo = User.Identity!.Name;
        var account = Repository.DataSet.FirstOrDefault(c => c.No == body.No);
        if (account == null) return NotFound();

        account.Name = body.Name;
        account.Identifier = body.Identifier;
        account.Phone = body.Phone;
        account.Email = body.Email;
        account.Expiration = body.Expiration;
        account.Status = body.Status;
        var updateAccount = Repository.UpdateAsync(account);
        var AuditEvent = AuditEventService.Add(AuditEventType.Update, account, TokenManager.Operationer(User.Identity!.Name));
        await Task.WhenAll(updateAccount, AuditEvent);
        return Ok("帳號更新成功");
    }

    /// <summary>
    /// 檢核代碼重複
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    protected override bool CreateValidation(AccNewModel model)
    {
        if (!Repository.DataSet.Any(c => c.No.ToLower() == model.No.ToLower())) 
            return ModelState.IsValid;

        ValidationMessage = "使用者代碼重複，請修改！";
        return false;
    }

    /// <summary>
    /// 解鎖帳號
    /// </summary>
    /// <param name="no">帳號</param>
    /// <returns></returns>
    [HttpGet("/Account/Unlock")]
    public ActionResult Unlock(string no)
    {
        var acc = Repository.DataSet.FirstOrDefault(c => c.No == no);
        if (acc == null) return BadRequest("無此帳號");
        if (acc.Status == 100) return BadRequest("無需解鎖");
        acc.Status = 100;
        acc.TryCount = 0;
        Repository.Update(acc);

        return Ok("解鎖成功");
    }

    /// <summary>
    /// 重置密碼
    /// </summary>
    /// <param name="No"></param>
    /// <returns></returns>
    [HttpPut("/Account/Reset")]
    public async Task<IActionResult> ResetPassword(string No)
    {
        var account = Repository.DataSet.FirstOrDefault(a => a.No == No);
        if (account == null)
            return Unauthorized("帳號不存在！");
        account.HashPermit = TokenManager.ToHash(No);
        account.LastLoginTime = null;
        account.PermitChangeTime = DateTime.Now.AddDays(90);
        //account.Status = 100;
        await Repository.UpdateAsync(account);
        await AuditEventService.Add(AuditEventType.ResetPassword, "重置密碼成功", $"PRAC.{TokenManager.UsingInstitutionNo}.{User.Identity!.Name}");
        return NoContent();
    }

    /// <summary>
    /// 稽核
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public AccountController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}