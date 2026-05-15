using Database.Core;
using Database.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Api.Entities;

namespace Api.Repositories;

/// <summary>
/// 報告檔
/// </summary>
public class ExamDataRepository : RepositoryBase<Db, ExamData>
{

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="repository">引用其他存儲管理物件連線</param>
    /// <param name="accessor">稽核記錄取得登入帳號及用戶端IP用</param>
    public ExamDataRepository(IRepository repository = null, IHttpContextAccessor accessor = null) : base(repository, accessor)
    {
    }
}