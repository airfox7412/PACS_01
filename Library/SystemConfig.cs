using Microsoft.Extensions.Configuration;

namespace Api.Library;

/// <summary>
/// 
/// </summary>
public static class SystemConfig
{
    private static IConfigurationRoot configuration;
    static SystemConfig()
    {
        var JsonFile = "appsettings.json";
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile(JsonFile, true, true);
        configuration = configurationBuilder.Build();
    }
    /// <summary>
    /// 密碼錯誤三次帳戶-暫停使用
    /// </summary>
    public static bool Error3Times => bool.Parse(configuration["Error3Times"]);
    /// <summary>
    /// 檢查帳戶三個月未使用-暫停使用
    /// </summary>
    public static bool Unused3Month => bool.Parse(configuration["Unused3Month"]);
    /// <summary>
    /// WebSite
    /// </summary>
    public static string WebSite => configuration["WebSite"];
    /// <summary>
    /// HistoryUrl
    /// </summary>
    public static string HistoryUrl => configuration["HistoryUrl"];
    /// <summary>
    /// 幾小時一次
    /// </summary>
    public static string HistoryTimes => configuration["HistoryTimes"];
    /// <summary>
    /// 幾分鐘一次
    /// </summary>
    public static string AITimes => configuration["AITimes"];
    /// <summary>
    /// Gemini API Key
    /// </summary>
    public static string GeminiApiKey => configuration["GeminiSettings:ApiKey"];

    /// <summary>
    /// Gemini API 終端節點 URL
    /// </summary>
    public static string GeminiEndpointUrl => configuration["GeminiSettings:EndpointUrl"];
    /// <summary>
    /// 將上傳影像的位置
    /// </summary>
    public static string UploadFiles => configuration["UploadFiles"];
    /// <summary>
    /// OT建議參考
    /// </summary>
    public static string OT_Recommand => configuration["OT_Recommand"];
}