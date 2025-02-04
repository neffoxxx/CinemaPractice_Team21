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

namespace CinemaPractice.Controllers
{
    public class FilmController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IRepository<Movie> _movieRepository;
        private readonly ILogger<FilmController> _logger;

        public FilmController(IMovieService movieService, IRepository<Movie> movieRepository, ILogger<FilmController> logger)
        {
            _movieService = movieService;
            _movieRepository = movieRepository;
            _logger = logger;
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

                var movie = await _movieRepository.GetByIdWithIncludeAsync(id,
                    query => query
                        .Include(m => m.MovieGenres)
                            .ThenInclude(mg => mg.Genre)
                        .Include(m => m.MovieActors)
                            .ThenInclude(ma => ma.Actor)
                        .Include(m => m.Sessions)
                            .ThenInclude(s => s.Hall));

                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {Id} not found", id);
                    return NotFound();
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
