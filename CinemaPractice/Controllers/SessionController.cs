using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaPractice.Controllers
{
    public class SessionController : Controller
    {
        private readonly IRepository<Session> _sessionRepository;

        public SessionController(IRepository<Session> sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var sessions = await _sessionRepository.GetAllAsync();
            return View(sessions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            return View(session);
        }
    }
}
