using Dapper;
using DBScriptDeployment.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DBScriptDeployment.Services
{
    public class UserService : IUserService
    {
        private const string BASE_INITIAL_CATALOG = "Autosales";
        private readonly string _secret;

        public UserService(string secret)
        {
            _secret = secret;
        }

        public TokenModel Authenticate(User user)
        {
            string connectionString = BuildConnectionString(user.Server, user.Username, user.Password);
            if (!IsServerConnected(connectionString, out int id))
                throw new UnauthorizedAccessException("User credentials are wrong");

            var tokenHandler = new JwtSecurityTokenHandler();

            var expires = DateTime.UtcNow.AddDays(7);

            var key = Encoding.ASCII.GetBytes(_secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.UserData, connectionString),
                    new Claim(ClaimTypes.Name, id.ToString())
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new TokenModel()
            {
                Token = tokenHandler.WriteToken(token),
                ExpiresTime = expires,
                Server = user.Server,
                Username = user.Username
            };
        }

        private static string BuildConnectionString(string server, string login, string password)
        {
            var sqlBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = BASE_INITIAL_CATALOG,
                UserID = login,
                Password = password
            };

            return sqlBuilder.ToString();
        }


        private static bool IsServerConnected(string connectionString, out int id)
        {
            using var connect = new SqlConnection(connectionString);
            try
            {
                connect.Open();
                id = connect.QuerySingle<int>(@"select hei.EmployeeID 
                                                 from HrEmployeeInfo as hei
                                                 where hei.UserID = dbo.[User.CurrentID]()");
                return true;
            }
            catch (SqlException e)
            {
                id = -1;
                return false;
            }
        }


    }
}