using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// 使用者帳號
/// </summary>
public class AccountModel
{
    /// <summary>
    /// 使用者帳號
    /// </summary>
    public string No { get; set; }

    /// <summary>
    /// 帳號姓名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 狀態碼
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 帳號到期日期
    /// </summary>
    public DateTime? Expiration { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    /// <summary>
    /// 醫事卡綁定狀態
    /// </summary>
    public bool Bind { get; set; }

    /// <summary>
    /// 所屬機構
    /// </summary>
    public virtual IList<AccSubInstModel> AccountSubInstitutions { get; set; }
    
    /// <summary>
    /// 狀態名稱
    /// </summary>
    public string StatusName { get; set; }
    /// <summary>
    /// 使用者身分證號
    /// </summary>
    public string Identifier { get; set; }
}

public class AccSubInstModel
{
    public string AccountNo { get; set; }
    public string OrgCode { get; set; }
    public string RoleNo { get; set; }
    public string OrgName { get; set; }
    public string RoleName { get; set; }
    public bool IsActive { get; set; }
    public string IsActiveName { get; set; }
}

/// <summary>
/// 使用者Model
/// </summary>
public class AccReadModel
{
    /// <summary>
    /// 使用者帳號
    /// </summary>
    [StringLength(30)]
    [Required()]
    public string No { get; set; }
    /// <summary>
    /// 身分證號
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// 帳號姓名
    /// </summary>
    [StringLength(30)]
    public string Name { get; set; }

    /// <summary>
    /// 聯絡電話
    /// </summary>
    [StringLength(30)]
    public string Phone { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    public string GenderCode { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [StringLength(100)]
    public string Email { get; set; }

    /// <summary>
    /// 體重
    /// </summary>
    public string Weight { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 帳號有效日期
    /// </summary>
    public DateTime? Expiration { get; set; }

    /// <summary>
    /// 帳號狀態
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 醫師兼診醫院機構代碼
    /// </summary>
    public List<AccSubInstReadModel> AccountSubInstitutions { get; set; }
}

public class AccSubInstReadModel
{
    /// <summary>
    /// 機構代碼
    /// </summary>
    [Required()]
    public string OrgCode { get; set; }
    /// <summary>
    /// 單位角色
    /// </summary>
    [Required()]
    public string RoleNo { get; set; }
    /// <summary>
    /// 科別
    /// </summary>
    [Required()]
    public string Specialty { get; set; }
    /// <summary>
    /// 機構啟用狀態
    /// </summary>
    public bool IsActive { get; set; }
}