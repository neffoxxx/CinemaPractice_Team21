using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using AppCore.Services; // Import Session Service
using AppCore.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaPractice.Controllers
{
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IGenreService _genreService; // Inject Genre Service

        public SessionController(ISessionService sessionService, IGenreService genreService)
        {
            _sessionService = sessionService;
            _genreService = genreService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? genreId)
        {
            // Get a list of genres for the filter dropdown
            var genres = await _genreService.GetAllGenresAsync();
            ViewBag.Genres = genres;  // Pass genres to the view

            // Get the sessions with optional filtering
            IEnumerable<SessionDTO> sessions = await _sessionService.GetFilteredSessionsAsync(startDate, endDate, genreId);

            return View(sessions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var session = await _sessionService.GetSessionByIdWithDetailsAsync(id);  // Use SessionService
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }
    }
}