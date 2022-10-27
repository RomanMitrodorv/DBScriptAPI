using DBScriptDeployment.Models;

namespace DBScriptDeployment.Services
{
    public interface IUserService
    {
        TokenModel Authenticate(User user);
    }
}
