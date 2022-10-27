
namespace DBScriptDeployment.Services
{
    public interface ITFSApiClient
    {
        Task<string> GetFileValue(string path);
    }
}
