using System.Collections.Generic;
using Database.Core.Models;

namespace Api.Models;

/// <summary>
/// 登入回傳
/// </summary>
public class UserLoginResult
{
    /// <summary>
    /// 使用者帳號
    /// </summary>        
    public string No { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>        
    public string Name { get; set; }

    /// <summary>
    /// 作業代碼
    /// </summary>
    /// <value>The functions.</value>
    public string[] FunctionNos { get; set; }

    /// <summary>
    /// 角色代碼
    /// </summary>
    /// <value>The functions.</value>
    public string[] RoleNos { get; set; }

    /// <summary>
    /// 權杖
    /// </summary>
    /// <value>The token.</value>
    public string Teken { get; set; }

    /// <summary>
    /// 是否首次登入
    /// </summary>
    public bool IsFirstLogin { get; set; }
        /// <summary>
    /// 超級使用者
    /// </summary>
    public bool IsSuper { get; set; }
    /// <summary>
    /// 身分證字號
    /// </summary>
    public string Identifier { get; set; } 
}