namespace Api.Models;

/// <summary>
/// 驗證圖
/// </summary>
public class CaptchaModel
{
    /// <summary>
    /// 驗證代碼
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 驗證圖
    /// </summary>
    public string Image { get; set; }
    /// <summary>
    /// 驗證Hash代碼
    /// </summary>
    public string HashCode { get; set; }
}