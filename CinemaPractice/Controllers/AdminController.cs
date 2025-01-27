using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using CinemaPractice.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CinemaPractice.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Session> _sessionRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IRepository<Movie> movieRepository, 
            IRepository<Session> sessionRepository,
            ILogger<AdminController> logger)
        {
            _movieRepository = movieRepository;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> ManageFilms()
        {
            var films = await _movieRepository.GetAllAsync();
            return View(films);
        }

        [HttpGet]
        public IActionResult AddFilm()
        {
            var movie = new Movie
            {
                ReleaseDate = DateTime.Today,
                Rating = 0,
                MovieActors = new List<MovieActor>(),
                MovieGenres = new List<MovieGenre>(),
                Sessions = new List<Session>()
            };
            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> AddFilm([FromForm] Movie movie)
        {
            try
            {
                _logger.LogInformation("Attempting to add new film: {Title}", movie.Title);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid ModelState: {Errors}", string.Join(", ", errors));

                    // Видаляємо помилки валідації для колекцій
                    ModelState.Remove("MovieActors");
                    ModelState.Remove("MovieGenres");
                    ModelState.Remove("Sessions");

                    if (!ModelState.IsValid)
                    {
                        return View(movie);
                    }
                }

                // Ініціалізуємо колекції
                movie.MovieActors = new List<MovieActor>();
                movie.MovieGenres = new List<MovieGenre>();
                movie.Sessions = new List<Session>();

                _logger.LogInformation("Adding movie to database");
                await _movieRepository.AddAsync(movie);
                
                _logger.LogInformation("Movie added successfully");
                TempData["Success"] = "Film added successfully";
                return RedirectToAction(nameof(ManageFilms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding film");
                ModelState.AddModelError("", $"Error adding film: {ex.Message}");
                return View(movie);
            }
        }

        public async Task<IActionResult> ManageSessions()
        {
            try
            {
                var sessions = await _sessionRepository.GetAllWithIncludeAsync(
                    query => query.Include(s => s.Movie));

                return View(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions");
                TempData["Error"] = "Error loading sessions";
                return View(new List<Session>());
            }
        }

        public async Task<IActionResult> AddSession()
        {
            var movies = await _movieRepository.GetAllAsync();
            var viewModel = new SessionViewModel
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                Movies = new SelectList(movies, "Id", "Title")
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddSession(SessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var session = new Session
                {
                    MovieId = model.MovieId,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Hall = model.Hall,
                    Price = model.Price
                };

                try
                {
                    await _sessionRepository.AddAsync(session);
                    TempData["Success"] = "Session added successfully";
                    return RedirectToAction(nameof(ManageSessions));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error adding session: " + ex.Message);
                }
            }

            var movies = await _movieRepository.GetAllAsync();
            model.Movies = new SelectList(movies, "Id", "Title");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFilm(int id)
        {
            try
            {
                await _movieRepository.DeleteAsync(id);
                TempData["Success"] = "Film deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting film: " + ex.Message;
            }
            return RedirectToAction(nameof(ManageFilms));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSession(int id)
        {
            try
            {
                await _sessionRepository.DeleteAsync(id);
                TempData["Success"] = "Session deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting session: " + ex.Message;
            }
            return RedirectToAction(nameof(ManageSessions));
        }

        public async Task<IActionResult> TestData()
        {
            var movie = new Movie
            {
                Title = "Test Movie",
                Description = "Test Description",
                DurationMinutes = 120
            };
            await _movieRepository.AddAsync(movie);

            var session = new Session
            {
                MovieId = movie.Id,
                StartTime = DateTime.Now.AddDays(1),
                Price = 100
            };
            await _sessionRepository.AddAsync(session);

            return RedirectToAction(nameof(ManageFilms));
        }

        [HttpGet]
        public async Task<IActionResult> EditFilm(int id)
        {
            var film = await _movieRepository.GetByIdAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            return View(film);
        }

        [HttpPost]
        public async Task<IActionResult> EditFilm(Movie movie)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _movieRepository.UpdateAsync(movie);
                    TempData["Success"] = "Film updated successfully";
                    return RedirectToAction(nameof(ManageFilms));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating film: " + ex.Message);
                }
            }
            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> EditSession(int id)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var movies = await _movieRepository.GetAllAsync();
            var viewModel = new SessionViewModel
            {
                SessionId = session.SessionId,
                MovieId = session.MovieId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Hall = session.Hall,
                Price = session.Price,
                Movies = new SelectList(movies, "Id", "Title", session.MovieId)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditSession(int id, SessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var session = await _sessionRepository.GetByIdAsync(id);
                    if (session == null)
                    {
                        return NotFound();
                    }

                    session.MovieId = model.MovieId;
                    session.StartTime = model.StartTime;
                    session.EndTime = model.EndTime;
                    session.Hall = model.Hall;
                    session.Price = model.Price;

                    await _sessionRepository.UpdateAsync(session);
                    TempData["Success"] = "Session updated successfully";
                    return RedirectToAction(nameof(ManageSessions));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating session: " + ex.Message);
                }
            }

            var movies = await _movieRepository.GetAllAsync();
            model.Movies = new SelectList(movies, "Id", "Title", model.MovieId);
            return View(model);
        }
    }
}
