using System.Threading.Tasks;

namespace Api.Library
{
    public interface IBoneDensityAnalyzer
    {
        Task<string> AnalyzeReportAsync(string imagePath);
    }

    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
    }

}
