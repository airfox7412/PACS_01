using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// 修改角色
/// </summary>
public class RoleModifyModel
{
    /// <summary>
    /// 角色代碼
    /// </summary>
    public virtual string No { get; set; }

    /// <summary>
    /// 角色名稱
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Required()]
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// 角色類型(0:授權角色, 1:職務角色)
    /// </summary>
    public virtual Enums.RoleType Type { get; set; }

}

/// <summary>
/// 角色
/// </summary>
public class RoleModel : RoleModifyModel
{
    /// <summary>
    /// 角色類型(一般角色, 職務角色)
    /// </summary>
    public virtual string TypeName { get; set; }
}

/// <summary>
/// 編輯授權使用者
/// </summary>
public class RoleAccountModifyModel
{
    /// <summary>
    /// 角色代碼
    /// </summary>
    public string RoleNo { get; set; }
    /// <summary>
    /// 使用者代碼
    /// </summary>
    public string AccountNo { get; set; }
}

/// <summary>
/// 編輯授權作業
/// </summary>
public class RoleFunctionModifyModel
{
    /// <summary>
    /// 角色代碼
    /// </summary>
    public string RoleNo { get; set; }
    /// <summary>
    /// 作業代碼
    /// </summary>
    public string FunctionNo { get; set; }
}