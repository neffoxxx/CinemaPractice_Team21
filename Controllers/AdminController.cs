using Microsoft.AspNetCore.Mvc;

namespace CinemaPractice.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult ManageFilms() // todo
        {
            return View();
        }
        public IActionResult ManageSessions() // todo
        {
            return View();
        }
    }
}
