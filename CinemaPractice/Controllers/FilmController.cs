using System.Diagnostics;
using CinemaPractice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Entities;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Interfaces;
using AppCore.Services;
using Microsoft.EntityFrameworkCore.Query;
using Infrastructure.Data;

namespace CinemaPractice.Controllers
{
    public class FilmController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IRepository<Movie> _movieRepository;
        private readonly ILogger<FilmController> _logger;
        private readonly CinemaDbContext _context;

        public FilmController(
            IMovieService movieService,
            IRepository<Movie> movieRepository,
            ILogger<FilmController> logger,
            CinemaDbContext context)
        {
            _movieService = movieService;
            _movieRepository = movieRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepository.GetAllAsync();
            return View(movies);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Entering Details action with ID: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid ID provided: {Id}", id);
                    return BadRequest();
                }

                var movie = await _context.Movies
                    .Include(m => m.MovieGenres)
                        .ThenInclude(mg => mg.Genre)
                    .Include(m => m.MovieActors)
                        .ThenInclude(ma => ma.Actor)
                    .Include(m => m.Sessions)
                        .ThenInclude(s => s.Hall)
                    .FirstOrDefaultAsync(m => m.MovieId == id);

                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {Id} not found", id);
                    return NotFound();
                }

                // Validate that all required relationships are present
                if (movie.Sessions.Any(s => s.Hall == null))
                {
                    _logger.LogError("Found session with null Hall reference for movie {Id}", id);
                    throw new InvalidOperationException("Hall cannot be null");
                }

                _logger.LogInformation("Successfully retrieved movie: {Title}", movie.Title);
                    
                return View(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie details for ID: {Id}", id);
                throw;
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
