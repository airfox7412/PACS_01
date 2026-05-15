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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Api.Controllers;

/// <summary>
/// 病患主檔
/// </summary>
[Authorize]
public class PatientMainController : ModelControllerBase<PatientMainRepository, PatientMain, 
    PatientMainModel, PatientMainNewModel, PatientMainModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override PatientMainModel ToModel(PatientMain entity)
    {
        var model = entity.Adapt<PatientMainModel>();
        return model;
    }

    /// <summary>
    /// 新增病患基本資料
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<IActionResult> Post([FromBody] PatientMainNewModel body)
    {
        try
        {
            var pat = Repository.DataSet.FirstOrDefault(c => c.PatientID == body.PatientID);
            if (pat == null)
            {
                var model = new PatientMain();
                model.PatientID = body.PatientID;
                model.PatientName = body.PatientName;
                model.PatientBirthDate = body.PatientBirthDate;
                model.PatientSex = body.PatientSex;
                await Repository.InsertAsync(model);
            }
            else
            {
                pat.PatientID = body.PatientID;
                pat.PatientName = body.PatientName;
                pat.PatientBirthDate = body.PatientBirthDate;
                pat.PatientSex = body.PatientSex;
                await Repository.UpdateAsync(pat);
            }
            return Ok("成功");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public PatientMainController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}