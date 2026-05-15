using System.ComponentModel.DataAnnotations;

namespace Api.Enums;

/// <summary>
/// 帳戶狀態
/// </summary>
public enum AccountApproveStatus
{
    /// <summary>
    /// 拒絕
    /// </summary>
    [Display(Name = "帳戶已拒絕")]
    Reject,
    /// <summary>
    /// 待審核
    /// </summary>
    [Display(Name = "帳戶待審核")]
    Approving
}

public enum AccountStatus
{
    /// <summary>
    /// 停用
    /// </summary>
    [Display(Name = "帳戶已停用")]
    Disabled = 0,
    /// <summary>
    /// 帳戶暫停登入
    /// </summary>
    [Display(Name = "帳戶暫停登入")]
    Pending,
    /// <summary>
    /// 啟用
    /// </summary>
    [Display(Name = "帳戶啟用")]
    Enabled = 100
}

public enum AuditType: short
{
    /// <summary>
    /// 帳號簽核
    /// </summary>
    Signature = 6,
    /// <summary>
    /// 派遣
    /// </summary>
    Provenance = 7,
    /// <summary>
    /// 卡片簽核
    /// </summary>
    CardSignature = 8,
    /// <summary>
    /// 修改密碼
    /// </summary>
    ChangePwd = 105,
    /// <summary>
    /// 異常處理
    /// </summary>
    ExceptionProcess = 106,
}