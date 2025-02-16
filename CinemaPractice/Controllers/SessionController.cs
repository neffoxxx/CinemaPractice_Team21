using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Data;
using AppCore.DTOs;
using AppCore.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaPractice.Controllers
{
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IGenreService _genreService;
        private readonly IHallService _hallService;
        private readonly IMovieService _movieService;

        public SessionController(ISessionService sessionService, IGenreService genreService, IHallService hallService, IMovieService movieService)
        {
            _sessionService = sessionService;
            _genreService = genreService;
            _hallService = hallService;
            _movieService = movieService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? genreId, decimal? minPrice, decimal? maxPrice, int? hallId, string movieTitle)
        {
            // Отримуємо дані для фільтрів.
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            ViewBag.Halls = await _hallService.GetAllHallsAsync();
            ViewBag.Movies = await _movieService.GetAllMoviesAsync();

            // Отримуємо відфільтровані сеанси
            var sessions = await _sessionService.GetFilteredSessionsAsync(startDate, endDate, genreId, minPrice, maxPrice, hallId, movieTitle);

            return View(sessions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var session = await _sessionService.GetSessionByIdWithDetailsAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }
    }
}