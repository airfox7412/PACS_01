using Database.Core;
using Database.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Api.Entities;
using Database.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Api.Repositories;

/// <summary>
/// 密碼修改紀錄
/// </summary>
public class PwdchangelogRepository : RepositoryBase<Db, Pwdchangelog>
{
    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="repository">引用其他存儲管理物件連線</param>
    /// <param name="accessor">稽核記錄取得登入帳號及用戶端IP用</param>
    public PwdchangelogRepository(IRepository repository = null, IHttpContextAccessor accessor = null) : base(repository, accessor)
    {
    }
}