namespace DBScriptDeployment.Services
{
    public interface IDBService
    {
        Task ExecuteScript(string script, string connectionString);
    }
}
