using Microsoft.AspNetCore.Mvc;

namespace DBScriptDeployment.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}
