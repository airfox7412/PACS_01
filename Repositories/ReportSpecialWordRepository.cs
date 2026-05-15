using Database.Core;
using Database.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Api.Entities;

namespace Api.Repositories;

/// <summary>
/// 特殊代碼檔
/// </summary>
public class ReportSpecialWordRepository : RepositoryBase<Db, ReportSpecialWord>
{

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="repository">引用其他存儲管理物件連線</param>
    /// <param name="accessor">稽核記錄取得登入帳號及用戶端IP用</param>
    public ReportSpecialWordRepository(IRepository repository = null, IHttpContextAccessor accessor = null) : base(repository, accessor)
    {
    }
}