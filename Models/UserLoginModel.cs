using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// 登入帳號密碼
/// </summary>
public class UserLoginModel
{
    /// <summary>
    /// 帳號
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 密碼
    /// </summary>
    public string Permit { get; set; }
    /// <summary>
    /// 驗證Hash代碼
    /// </summary>
    [Required]
    public string CaptchaHashCode { get; set; }

}

/// <summary>
/// UserInfoModel
/// </summary>
public class UserInfoModel
{
    /// <summary>
    /// 帳號
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 單位代碼
    /// </summary>
    //[Required]
    public string OrgId { get; set; }

}