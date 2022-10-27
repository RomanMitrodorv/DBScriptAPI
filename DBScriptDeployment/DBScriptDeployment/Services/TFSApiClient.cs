//using TFSClient;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using File = DBScriptDeployment.Models.File;

namespace DBScriptDeployment.Services
{
    public class TFSApiClient : ITFSApiClient
    {
        private const string BASE_PATH_PRODUCTION = "$/TEAM_A/AutoSales-PRODUCTION/";
        private const string BASE_PATH = "$/TEAM_A/AutoSales/";
        private readonly HttpClient _apiClient;
        private readonly string url;

        public TFSApiClient(HttpClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            url = configuration["TFS:ClientUrl"];
        }

        public async Task<string> GetFileValue(string path)
        {
            if (!IsValidFilePath(path))
                throw new ArgumentException("Path is not valid");

            path = FormatPath(path);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = new StringContent(JsonSerializer.Serialize(new { path }), Encoding.UTF8, MediaTypeNames.Application.Json),
            };

            var response = await _apiClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var fileValue = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<File>(fileValue).Value;
        }

        private static bool IsValidFilePath(string path)
        {
            return path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        }

        private static string FormatPath(string path, bool isProd = false)
        {
            var value = path;

            if (value.StartsWith("DB") || value.StartsWith("\\DB"))
                value = isProd ? BASE_PATH_PRODUCTION : BASE_PATH + value;

            value = value.Replace('\\', '/');

            return value;
        }

    }
}
