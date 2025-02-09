using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppCore.DTOs;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppCore.Interfaces;
using System.Collections.Generic;

namespace CinemaPractice.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<AdminController> _logger;
        private readonly ITicketService _ticketService;
        private readonly IHallService _hallService;
        private readonly IValidator<MovieDTO> _movieValidator;
        private readonly IValidator<SessionDTO> _sessionValidator;
        private readonly IValidator<TicketDTO> _ticketValidator;
        private readonly IValidator<HallDTO> _hallValidator;
        private readonly IActorService _actorService;
        private readonly IGenreService _genreService;

        public AdminController(
            IMovieService movieService,
            ISessionService sessionService,
            ILogger<AdminController> logger,
            ITicketService ticketService,
            IHallService hallService,
            IValidator<MovieDTO> movieValidator,
            IValidator<SessionDTO> sessionValidator,
            IValidator<TicketDTO> ticketValidator,
            IValidator<HallDTO> hallValidator,
            IActorService actorService,
            IGenreService genreService)
        {
            _movieService = movieService;
            _sessionService = sessionService;
            _logger = logger;
            _ticketService = ticketService;
            _hallService = hallService;
            _movieValidator = movieValidator;
            _sessionValidator = sessionValidator;
            _ticketValidator = ticketValidator;
            _hallValidator = hallValidator;
            _actorService = actorService;
            _genreService = genreService;
        }

        public async Task<IActionResult> ManageFilms()
        {
            try
            {
                var movies = await _movieService.GetAllMoviesAsync();
                return View(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movies list");
                TempData["Error"] = "An error occurred while loading the movies list.";
                return View(new List<MovieDTO>());
            }
        }

        public async Task<IActionResult> AddFilm()
        {
            await LoadGenresAndActors();
            return View(new MovieDTO 
            { 
                Title = string.Empty,
                Description = string.Empty,
                Director = string.Empty,
                ReleaseDate = DateTime.Today
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddFilm(MovieDTO movieDto)
        {
            if (ModelState.IsValid)
            {
                await _movieService.AddMovieAsync(movieDto);
                return RedirectToAction(nameof(ManageFilms));
            }
            await LoadGenresAndActors();
            return View(movieDto);
        }

        public async Task<IActionResult> ManageSessions()
        {
            try
            {
                var sessions = await _sessionService.GetAllSessionsAsync();
                return View(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sessions");
                TempData["Error"] = "Error loading sessions: " + ex.Message;
                return View(new List<SessionDTO>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddSession()
        {
            var sessionDto = new SessionDTO
            {
                StartTime = DateTime.Now.AddHours(1).RoundToNearestHour()
            };
            
            await _sessionService.PopulateSessionSelectLists(sessionDto);
            return View(sessionDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddSession(SessionDTO sessionDto)
        {
            if (!ModelState.IsValid)
            {
                await _sessionService.PopulateSessionSelectLists(sessionDto);
                return View(sessionDto);
            }

            try
            {
                await _sessionService.AddSessionAsync(sessionDto);
                TempData["Success"] = "Session added successfully";
                return RedirectToAction(nameof(ManageSessions));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding session: {ex.Message}");
                await _sessionService.PopulateSessionSelectLists(sessionDto);
                return View(sessionDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFilm(int id)
        {
            try
            {
                await _movieService.DeleteMovieAsync(id);
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
                await _sessionService.DeleteSessionAsync(id);
                TempData["Success"] = "Session deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting session: " + ex.Message;
            }
            return RedirectToAction(nameof(ManageSessions));
        }

        public async Task<IActionResult> EditFilm(int id)
        {
            try
            {
                _logger.LogInformation("Loading movie for editing, ID: {Id}", id);

                // Load related data first
                try
                {
                    await LoadGenresAndActors();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading genres and actors");
                    TempData["Error"] = "Error loading genres and actors data.";
                    return RedirectToAction(nameof(ManageFilms));
                }

                // Get the movie
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    _logger.LogWarning("Movie not found, ID: {Id}", id);
                    TempData["Error"] = $"Movie with ID {id} not found.";
                    return RedirectToAction(nameof(ManageFilms));
                }

                _logger.LogInformation("Successfully loaded movie for editing: {Title} (ID: {Id})", movie.Title, id);
                return View(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movie for editing, ID: {Id}. Error: {Message}", id, ex.Message);
                TempData["Error"] = $"Error loading movie: {ex.Message}";
                return RedirectToAction(nameof(ManageFilms));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFilm(MovieDTO movieDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadGenresAndActors();
                    return View(movieDto);
                }

                var validationResult = await _movieValidator.ValidateAsync(movieDto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    await LoadGenresAndActors();
                    return View(movieDto);
                }

                await _movieService.UpdateMovieAsync(movieDto);
                TempData["Success"] = "Movie updated successfully.";
                return RedirectToAction(nameof(ManageFilms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie {MovieId}: {Message}", movieDto.MovieId, ex.Message);
                TempData["Error"] = $"Error updating movie: {ex.Message}";
                await LoadGenresAndActors();
                return View(movieDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditSession(int id)
        {
            // Використовуємо метод, який повертає сесію для редагування (з повними деталями)
            var session = await _sessionService.GetSessionForEditAsync(id);
            if (session == null)
            {
               return NotFound();
            }
            
            // Оновлюємо списки вибору (Movies та Halls)
            await _sessionService.PopulateSessionSelectLists(session);
            return View(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(SessionDTO sessionDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Повторно заповнюємо випадаючі списки з відфільтрованими даними
                    await _sessionService.PopulateSessionSelectLists(sessionDTO);
                    return View(sessionDTO);
                }

                // Отримуємо дані фільму для розрахунку EndTime, якщо потрібно
                var movie = await _movieService.GetMovieByIdAsync(sessionDTO.MovieId);
                if (movie != null)
                {
                    sessionDTO.EndTime = sessionDTO.StartTime.AddMinutes(movie.DurationMinutes);
                }

                await _sessionService.UpdateSessionAsync(sessionDTO);
                TempData["Success"] = "Session updated successfully.";
                return RedirectToAction(nameof(ManageSessions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session");
                ModelState.AddModelError("", "Error updating session.");

                // Повторно заповнюємо випадаючі списки для відображення коректних даних
                await _sessionService.PopulateSessionSelectLists(sessionDTO);
                return View(sessionDTO);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageTickets()
        {
            var tickets = await _ticketService.GetAllTicketsAsync();
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> EditTicket(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("ManageTickets");
            }
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTicket(TicketDTO model)
        {
            var validationResult = await _ticketValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View(model);
            }

            try
            {
                // Перевіряємо, чи змінилося місце
                var oldTicket = await _ticketService.GetTicketByIdAsync(model.TicketId);
                if (oldTicket == null)
                {
                    ModelState.AddModelError("", "Ticket not found");
                    return View(model);
                }

                if (oldTicket.RowNumber != model.RowNumber || oldTicket.SeatNumber != model.SeatNumber)
                {
                    // Перевіряємо, чи нове місце доступне
                    var isSeatAvailable = await _ticketService.IsSeatAvailable(
                        model.SessionId,
                        model.RowNumber,
                        int.Parse(model.SeatNumber));

                    if (!isSeatAvailable)
                    {
                        ModelState.AddModelError("", "This seat is already booked. Please select another seat.");
                        return View(model);
                    }
                }

                await _ticketService.UpdateTicketAsync(model);

                TempData["Success"] = "Ticket updated successfully.";
                return RedirectToAction("ManageTickets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", model.TicketId);
                ModelState.AddModelError("", "An error occurred while updating the ticket.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                await _ticketService.DeleteTicketAsync(id);
                TempData["Success"] = "Ticket deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket, ID: {Id}", id);
                TempData["Error"] = "An error occurred while deleting the ticket.";
            }

            return RedirectToAction(nameof(ManageTickets));
        }

        public async Task<IActionResult> ManageHalls()
        {
            var halls = await _hallService.GetAllHallsAsync();
            return View(halls);
        }
        public IActionResult AddHall()
        {
            return View(new HallDTO 
            { 
                Name = string.Empty,
                IsActive = true,
                Capacity = 0,
                RowsCount = 0,
                SeatsPerRow = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHall(HallDTO model)
        {
            var validationResult = await _hallValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(model);
            }

            try
            {
                await _hallService.AddHallAsync(model);
                _logger.LogInformation("Hall {HallName} created successfully", model.Name);
                TempData["Success"] = "Hall created successfully";
                return RedirectToAction(nameof(ManageHalls));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hall");
                ModelState.AddModelError("", "An error occurred while creating the hall");
                return View(model);
            }
        }
        public async Task<IActionResult> EditHall(int id)
        {
            var hall = await _hallService.GetHallByIdAsync(id);
            if (hall == null)
                return NotFound();

            return View(hall);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHall(HallDTO model)
        {
            var validationResult = await _hallValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(model);
            }

            try
            {
                await _hallService.UpdateHallAsync(model);
                _logger.LogInformation("Hall {HallId} updated successfully", model.HallId);
                TempData["Success"] = "Hall updated successfully";
                return RedirectToAction(nameof(ManageHalls));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hall {HallId}", model.HallId);
                ModelState.AddModelError("", "An error occurred while updating the hall");
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHall(int id)
        {
            try
            {
                await _hallService.DeleteHallAsync(id);
                _logger.LogInformation("Hall {HallId} deleted successfully", id);
                TempData["Success"] = "Hall deleted successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hall {HallId}", id);
                TempData["Error"] = "An error occurred while deleting the hall";
            }

            return RedirectToAction(nameof(ManageHalls));
        }

        // Actors
        public async Task<IActionResult> ManageActors()
        {
            var actors = await _actorService.GetAllActorsAsync();
            return View(actors);
        }

        public IActionResult AddActor()
        {
            return View(new ActorDTO 
            { 
                Name = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActor(ActorDTO actorDto)
        {
            if (ModelState.IsValid)
            {
                await _actorService.AddActorAsync(actorDto);
                return RedirectToAction(nameof(ManageActors));
            }
            return View(actorDto);
        }

        public async Task<IActionResult> EditActor(int id)
        {
            var actor = await _actorService.GetActorByIdAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActor(ActorDTO actorDto)
        {
            if (ModelState.IsValid)
            {
                await _actorService.UpdateActorAsync(actorDto);
                return RedirectToAction(nameof(ManageActors));
            }
            return View(actorDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActor(int id)
        {
            await _actorService.DeleteActorAsync(id);
            return RedirectToAction(nameof(ManageActors));
        }

        // Genres
        public async Task<IActionResult> ManageGenres()
        {
            var genres = await _genreService.GetAllGenresAsync();
            return View(genres);
        }

        public IActionResult AddGenre()
        {
            return View(new GenreDTO 
            { 
                Name = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGenre(GenreDTO genreDto)
        {
            if (ModelState.IsValid)
            {
                await _genreService.AddGenreAsync(genreDto);
                return RedirectToAction(nameof(ManageGenres));
            }
            return View(genreDto);
        }

        public async Task<IActionResult> EditGenre(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGenre(GenreDTO genreDto)
        {
            if (ModelState.IsValid)
            {
                await _genreService.UpdateGenreAsync(genreDto);
                return RedirectToAction(nameof(ManageGenres));
            }
            return View(genreDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            await _genreService.DeleteGenreAsync(id);
            return RedirectToAction(nameof(ManageGenres));
        }

        private async Task LoadGenresAndActors()
        {
            try
            {
                var genres = await _genreService.GetAllGenresAsync();
                var actors = await _actorService.GetAllActorsAsync();

                if (genres == null || actors == null)
                {
                    throw new InvalidOperationException("Failed to load genres or actors");
                }

                ViewBag.Genres = genres;
                ViewBag.Actors = actors;

                _logger.LogInformation("Successfully loaded {GenresCount} genres and {ActorsCount} actors", 
                    genres.Count(), actors.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoadGenresAndActors");
                throw;
            }
        }
    }
}