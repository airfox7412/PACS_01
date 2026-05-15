using Database.Core;
using Database.Core.Enums;
using Database.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Api.Entities;
using Api.Library;

namespace Api.Repositories;

/// <summary>
/// 使用者
/// </summary>
public class AccountRepository : RepositoryBase<Db, Account>
{
    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="repository">引用其他存儲管理物件連線</param>
    /// <param name="accessor">稽核記錄取得登入帳號及用戶端IP用</param>
    public AccountRepository(IRepository repository = null, IHttpContextAccessor accessor = null) : base(repository, accessor)
    {
    }

    /// <summary>
    /// 更新物件實體前置作業
    /// </summary>
    /// <param name="account">物件實體</param>
    /// <param name="entityState">更新類型</param>
    public override void BeforeModify(Account account, ModifyType entityState)
    {
        switch (entityState)
        {
            case ModifyType.Delete when account.IsSuper == true:
                throw new ValidationException("不可刪除系統管理員!");
            case ModifyType.Insert:
                account.CreateTime = DateTime.Now;
                account.HashPermit ??= TokenManager.ToHash(account.No);
                account.IsDeleted = false;
                account.TryCount = 0;
                account.PermitChangeTime = DateTime.Now.AddDays(-90);
                break;
            case ModifyType.Update:
                if (account.Status == 0)
                    account.TryCount = 0;
                break;
        }
        account.UpdateTime = DateTime.Now;
    }
}