using Database.Core;
using Database.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Api.Entities;
using Database.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Api.Repositories;

/// <summary>
/// 基本資料代碼表
/// </summary>
public class SystemcodeRepository : RepositoryBase<Db, Systemcode>
{
    /// <summary>
    /// 取得文字型常用資料集
    /// </summary>
    /// <param name="category">資料集類別</param>
    /// <returns></returns>
    public IEnumerable<SelectNoModel> GetTexts(int category) =>
        DataSet.Where(c => c.Category == category)
            .Select(c => new SelectNoModel()
        {
            No = c.Code,
            Name = c.Display
        });

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="repository">引用其他存儲管理物件連線</param>
    /// <param name="accessor">稽核記錄取得登入帳號及用戶端IP用</param>
    public SystemcodeRepository(IRepository repository = null, IHttpContextAccessor accessor = null) : base(repository, accessor)
    {
    }
}