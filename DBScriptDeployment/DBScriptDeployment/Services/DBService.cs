using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DBScriptDeployment.Services
{
    public class DBService : IDBService
    {
        private readonly ILogger<DBService> _logger;

        public DBService(ILogger<DBService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteScript(string script, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(script);


            if (connection.State != ConnectionState.Open)
                connection.Open();

            using SqlCommand command = connection.CreateCommand();
            try
            {
                command.Connection = connection;

                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        command.CommandText = line;
                        command.CommandType = CommandType.Text;
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException exp)
            {
                _logger.LogError(exp, $"Error execute command: {command.CommandText}");
                throw;
            }
        }

    }
}
