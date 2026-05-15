namespace Api.Models;

/// <summary>
/// 變更密碼
/// </summary>
public class ChangePermitModel
{
    /// <summary>
    /// 原密碼
    /// </summary>
    public string Old { get; set; }
    /// <summary>
    /// 新密碼
    /// </summary>
    public string New { get; set; }
}