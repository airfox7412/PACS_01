using System.ComponentModel.DataAnnotations;

namespace Api.Enums
{
    /// <summary>
    /// 角色類型
    /// </summary>
    public enum RoleType : byte
    {
        [Display(Name = "授權角色")]
        Auth = 1,
        [Display(Name = "職務角色")]
        Duty
    }
}
