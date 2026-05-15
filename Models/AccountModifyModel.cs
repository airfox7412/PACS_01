using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// 修改使用者帳號
/// </summary>
public class AccountModifyModel
{
    /// <summary>
    /// 帳號(員工代碼)
    /// </summary>
    [Key]
    [StringLength(30)]
    [Required()]
    public string No { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [StringLength(30)]
    public string Name { get; set; }

    /// <summary>
    /// 聯絡電話
    /// </summary>
    [StringLength(30)]
    public string Phone { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    [StringLength(100)]
    public string Email { get; set; }

    /// <summary>
    /// 使用者密碼
    /// </summary>
    [StringLength(64)]
    public string HashPermit { get; set; }

    /// <summary>
    /// 帳號有效日期
    /// </summary>
    public DateTime? Expiration { get; set; }

    /// <summary>
    /// 帳號狀態
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 是否為超級使用者
    /// </summary>
    public bool IsSuper { get; set; }

    /// <summary>
    /// 註記刪除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 密碼變更時間
    /// </summary>
    public DateTime? PermitChangeTime { get; set; }

    /// <summary>
    /// 密碼錯誤次數
    /// </summary>
    public int TryCount { get; set; }

    /// <summary>
    /// 帳號配置文件
    /// </summary>
    [StringLength(4000)]
    public string Profile { get; set; }

}

/// <summary>
/// 修改使用者帳號
/// </summary>
public class AccUpdateModel
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
    public IList<AccSubInstNewModel> AccountSubInstitutions { get; set; }
}

public class AccSubInstUpdateModel
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



/// <summary>
/// 新增使用者帳號
/// </summary>
public class AccNewModel
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
    [StringLength(30)]
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
    public IList<AccSubInstNewModel> AccountSubInstitutions { get; set; }
}

public class AccSubInstNewModel
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
    public string Specialty { get; set; }
    /// <summary>
    /// 機構啟用狀態
    /// </summary>
    public bool IsActive { get; set; }
}