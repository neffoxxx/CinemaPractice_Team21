using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppCore.DTOs;
using AppCore.Services;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var films = await _movieService.GetAllMoviesAsync();
            return View(films);
        }

        public async Task<IActionResult> AddFilm()
        {
            await LoadGenresAndActors();
            return View(new MovieDTO());
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
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            ViewBag.Actors = await _actorService.GetAllActorsAsync();

            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> EditFilm(MovieDTO movieDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _movieService.UpdateMovieAsync(movieDto);
                    return RedirectToAction(nameof(ManageFilms));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the movie.");
                    _logger.LogError(ex, "Error updating movie");
                }
            }

            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            ViewBag.Actors = await _actorService.GetAllActorsAsync();
            return View(movieDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditSession(int id)
        {
            try
            {
                var sessionDto = await _sessionService.GetSessionForEditAsync(id);
                if (sessionDto == null)
                {
                    return NotFound();
                }
                return View(sessionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading session for edit");
                TempData["Error"] = "Error loading session: " + ex.Message;
                return RedirectToAction(nameof(ManageSessions));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(SessionDTO sessionDto)
        {
            var validationResult = await _sessionValidator.ValidateAsync(sessionDto);
            if (!validationResult.IsValid)
            {
                await _sessionService.PopulateSessionSelectLists(sessionDto);
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(sessionDto);
            }

            try
            {
                await _sessionService.UpdateSessionAsync(sessionDto);
                TempData["Success"] = "Session updated successfully";
                return RedirectToAction(nameof(ManageSessions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session");
                ModelState.AddModelError("", "Error updating session: " + ex.Message);
                await _sessionService.PopulateSessionSelectLists(sessionDto);
                return View(sessionDto);
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
            return View(new HallDTO { IsActive = true });
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
            return View(new ActorDTO());
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
            return View(new GenreDTO());
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
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            ViewBag.Actors = await _actorService.GetAllActorsAsync();
        }
    }
}