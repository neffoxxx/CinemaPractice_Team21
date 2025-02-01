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

        public AdminController(
            IMovieService movieService,
            ISessionService sessionService,
            ILogger<AdminController> logger,
            ITicketService ticketService,
             IHallService hallService,
               IValidator<MovieDTO> movieValidator,
            IValidator<SessionDTO> sessionValidator,
            IValidator<TicketDTO> ticketValidator,
            IValidator<HallDTO> hallValidator)
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
        }


        public async Task<IActionResult> ManageFilms()
        {
            var films = await _movieService.GetAllMoviesAsync();
            return View(films);
        }

        [HttpGet]
        public IActionResult AddFilm()
        {
            return View(new MovieDTO { ReleaseDate = DateTime.Today, Rating = 0, Director = "" });
        }

        [HttpPost]
        public async Task<IActionResult> AddFilm(MovieDTO movieDto)
        {
            var validationResult = await _movieValidator.ValidateAsync(movieDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View(movieDto);
            }
            try
            {
                _logger.LogInformation("Attempting to add new film: {Title}", movieDto.Title);

                await _movieService.AddMovieAsync(movieDto);
                _logger.LogInformation("Movie added successfully");
                TempData["Success"] = "Film added successfully";
                return RedirectToAction(nameof(ManageFilms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding film");
                ModelState.AddModelError("", $"Error adding film: {ex.Message}");
                return View(movieDto);
            }
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

        public async Task<IActionResult> AddSession()
        {
            try
            {
                _logger.LogInformation("Loading data for AddSession form");
                var model = new SessionDTO();
                await _sessionService.PopulateSessionSelectLists(model);

                // Set initial time to the next hour rounded down
                var startTime = DateTime.Now.AddHours(1);
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
                model.StartTime = startTime;
                model.EndTime = startTime;

                if (model.Movies == null || !model.Movies.Any())
                {
                    ModelState.AddModelError("MovieId", "No movies available in the database");
                }
                if (model.Halls == null || !model.Halls.Any())
                {
                    ModelState.AddModelError("HallId", "No halls available in the database");
                }
                return View(model);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing AddSession form");
                TempData["Error"] = "Error loading form data: " + ex.Message;
                return RedirectToAction(nameof(ManageSessions));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSession(SessionDTO sessionDto)
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
                await _sessionService.AddSessionAsync(sessionDto);
                TempData["Success"] = "Session added successfully";
                return RedirectToAction(nameof(ManageSessions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding session");
                ModelState.AddModelError("", "Error adding session: " + ex.Message);
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

        [HttpGet]
        public async Task<IActionResult> EditFilm(int id)
        {
            try
            {
                _logger.LogInformation("Loading film for editing, ID: {Id}", id);
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound();
                }
                return View(movie);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading film for editing, ID: {Id}", id);
                TempData["Error"] = "Error loading film for editing: " + ex.Message;
                return RedirectToAction(nameof(ManageFilms));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFilm(MovieDTO movieDto)
        {
            var validationResult = await _movieValidator.ValidateAsync(movieDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(movieDto);
            }

            try
            {
                _logger.LogInformation("Attempting to update film: {Id}", movieDto.MovieId);
                await _movieService.UpdateMovieAsync(movieDto);
                _logger.LogInformation("Film updated successfully: {Id}", movieDto.MovieId);
                TempData["Success"] = "Film updated successfully";
                return RedirectToAction(nameof(ManageFilms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating film: {Id}", movieDto.MovieId);
                ModelState.AddModelError("", "Error updating film: " + ex.Message);
                return View(movieDto);
            }
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
    }
}