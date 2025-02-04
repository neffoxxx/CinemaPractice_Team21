using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace CinemaPractice.Controllers
{
    public class SessionController : Controller
    {
        private readonly IRepository<Session> _sessionRepository;
        private readonly CinemaDbContext _context;

        public SessionController(IRepository<Session> sessionRepository, CinemaDbContext context)
        {
            _sessionRepository = sessionRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
            return View(sessions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }
    }
}
