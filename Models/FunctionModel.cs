using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// 作業
/// </summary>
public class FunctionModel
{
    /// <summary>
    /// 作業代碼
    /// </summary>
    [Key]
    [StringLength(50)]
    [Required()]
    public virtual string No { get; set; }

    /// <summary>
    /// 作業名稱
    /// </summary>
    [StringLength(50)]
    [Required()]
    public virtual string Name { get; set; }

    /// <summary>
    /// 顯示順序
    /// </summary>
    public virtual int? Seq { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    [Required()]
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// 允許之API
    /// </summary>
    [StringLength(255)]
    public string AllowApi { get; set; }

}