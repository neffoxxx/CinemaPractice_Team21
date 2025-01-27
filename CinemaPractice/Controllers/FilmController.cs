using System.Diagnostics;
using CinemaPractice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Entities;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CinemaPractice.Controllers
{
    public class FilmController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly ILogger<FilmController> _logger;

        public FilmController(IRepository<Movie> movieRepository, ILogger<FilmController> logger)
        {
            _movieRepository = movieRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepository.GetAllAsync();
            return View(movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Getting movie details for id: {Id}", id);

                var movie = await _movieRepository.GetByIdWithIncludeAsync(id, 
                    query => query.Include(m => m.Sessions.OrderBy(s => s.StartTime)));

                if (movie == null)
                {
                    _logger.LogWarning("Movie with id {Id} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Movie found: {Title}", movie.Title);
                _logger.LogInformation("Total sessions: {Count}", movie.Sessions?.Count ?? 0);

                // Фільтруємо сесії, щоб показувати тільки майбутні
                if (movie.Sessions != null)
                {
                    var currentTime = DateTime.Now;
                    movie.Sessions = movie.Sessions
                        .Where(s => s.StartTime > currentTime)
                        .OrderBy(s => s.StartTime)
                        .ToList();

                    _logger.LogInformation("Future sessions: {Count}", movie.Sessions.Count);
                }

                return View(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movie details: {Message}", ex.Message);
                return RedirectToAction("Index", "Home");
            }
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
