using DBScriptDeployment.Models;
using DBScriptDeployment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBScriptDeployment.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly IUserService _userService;

        public AccountController(ILogger<TaskController> logger, IUserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            try
            {
                var token = _userService.Authenticate(user);

                return Ok(token);
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogError(exception, $"Failed auth to databse: {user.Server}, {user.Username}!");
                return Unauthorized(new ErrorView("Login failed", "Can't connect to database. Invalid credentials. "));
            }
        }
    }
}
