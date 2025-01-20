using System.Diagnostics;
using CinemaPractice.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaPractice.Controllers
{
    public class FilmController : Controller
    {
        private readonly ILogger<FilmController> _logger;

        public FilmController(ILogger<FilmController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
