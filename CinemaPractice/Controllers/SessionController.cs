using Microsoft.AspNetCore.Mvc;

namespace CinemaPractice.Controllers
{
    public class SessionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
