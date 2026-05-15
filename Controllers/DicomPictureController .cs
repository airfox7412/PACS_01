using Database.Core;
using Database.Core.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Api.Entities;
using Api.Models;
using Api.Repositories;
using NLog;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers;

/// <summary>
/// 圖片檔
/// </summary>
[Authorize]
public class DicomPictureController : ControllerBase<DicomPictureRepository, DicomPicture, DicomPictureModel>
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    //獲取應用程式所在目錄（絕對，不受工作目錄影響，建議採用此方法獲取路徑)
    private static string basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override DicomPictureModel ToModel(DicomPicture entity)
    {
        var model = entity.Adapt<DicomPictureModel>();
        return model;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
#if DEBUG
    [AllowAnonymous]
#endif
    [HttpGet("/DicomPicture/DelMoreRec")]
    public IActionResult DelMoreRec()
    {
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        var configuration = configurationBuilder.Build();
        var connString = configuration.GetConnectionString("Db");
        
        using (var conn = new MySqlConnection(connString))
        {
            var SqlStr = "SELECT * FROM dicom_picture ";
            SqlStr += "GROUP BY image ";
            SqlStr += "HAVING COUNT(image) > 1 ";
            SqlStr += "LIMIT 5000";
            var dps = conn.Query<DicomPicture>(SqlStr).ToList();
            foreach(var i in dps)
            {
                System.Diagnostics.Debug.WriteLine(i);
            }
        };

        return Ok();
    }



    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public DicomPictureController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}