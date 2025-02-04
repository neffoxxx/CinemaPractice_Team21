using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CinemaPractice.Models;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using AppCore.Services;

namespace CinemaPractice.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(
            ITicketRepository ticketRepository,
            ISessionRepository sessionRepository,
            ITicketService ticketService,
            ILogger<TicketController> logger)
        {
            _ticketRepository = ticketRepository;
            _sessionRepository = sessionRepository;
            _ticketService = ticketService;
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> Book(int sessionId)
        {
            try
            {
                _logger.LogInformation("Starting booking process for session: {SessionId}", sessionId);

                if (sessionId <= 0)
                {
                    _logger.LogWarning("Invalid session ID: {SessionId}", sessionId);
                    return BadRequest("Invalid session ID");
                }

                var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId);
                
                if (session == null)
                {
                    _logger.LogWarning("Session not found: {SessionId}", sessionId);
                    return NotFound("Session not found");
                }

                // Перевірка наявності зв'язаних даних
                if (session.Movie == null)
                {
                    _logger.LogError("Movie data not found for session: {SessionId}", sessionId);
                    return NotFound("Movie data not found");
                }

                if (session.Hall == null)
                {
                    _logger.LogError("Hall data not found for session: {SessionId}", sessionId);
                    return NotFound("Hall data not found");
                }

                // Отримуємо тільки ряди з вільними місцями
                var availableRows = await _ticketService.GetRowsWithAvailableSeats(
                    sessionId, 
                    session.Hall.RowsCount, 
                    session.Hall.SeatsPerRow);

                _logger.LogInformation("Available rows for session {SessionId}: {Rows}", 
                    sessionId, string.Join(", ", availableRows));

                var existingTickets = await _ticketRepository.GetTicketsBySessionAsync(sessionId);
                var bookedSeats = existingTickets.Select(t => t.SeatNumber).ToList();

                _logger.LogInformation("Session {SessionId} details:" +
                    "\nTotal Rows: {RowsCount}" +
                    "\nSeats Per Row: {SeatsPerRow}" +
                    "\nBooked Seats Count: {BookedSeatsCount}" +
                    "\nAvailable Rows Count: {AvailableRowsCount}",
                    sessionId,
                    session.Hall.RowsCount,
                    session.Hall.SeatsPerRow,
                    bookedSeats.Count,
                    availableRows.Count);

                var model = new TicketBookingViewModel
                {
                    SessionId = sessionId,
                    Price = session.Price,
                    ShowTime = session.StartTime,
                    MovieTitle = session.Movie?.Title,
                    HallId = session.HallId,
                    HallName = session.Hall?.Name,
                    RowsCount = session.Hall.RowsCount,
                    SeatsPerRow = session.Hall.SeatsPerRow,
                    BookedSeats = bookedSeats,
                    AvailableRows = availableRows  // Перевіряємо, чи не null тут
                };

                _logger.LogInformation("Created TicketBookingViewModel for session {SessionId}:" +
                    "\nAvailable Rows: {AvailableRows}" +
                    "\nBooked Seats: {BookedSeats}",
                    sessionId,
                    string.Join(", ", model.AvailableRows),
                    string.Join(", ", model.BookedSeats));

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking page for session {SessionId}", sessionId);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(TicketBookingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    ModelState.AddModelError("", "User authentication error.");
                    return View(model);
                }

                // Обчислюємо позицію місця
                var (rowNumber, seatInRow) = await _ticketService.CalculateSeatPosition(
                    int.Parse(model.SeatNumber), 
                    model.SeatsPerRow);

                // Перевіряємо, чи місце доступне
                if (!await _ticketService.IsSeatAvailable(model.SessionId, rowNumber, seatInRow))
                {
                    ModelState.AddModelError("", "This seat is already booked. Please select another seat.");
                    return View(model);
                }

                var ticket = new Ticket
                {
                    SessionId = model.SessionId,
                    UserId = userId,
                    RowNumber = rowNumber,
                    SeatNumber = seatInRow.ToString(),
                    BookingTime = DateTime.Now,
                    Status = "Booked"
                };

                await _ticketRepository.AddAsync(ticket);
                TempData["Success"] = "Ticket booked successfully!";
                return RedirectToAction("MyTickets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking ticket");
                ModelState.AddModelError("", "An error occurred while booking the ticket.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSeats(int sessionId, int rowNumber, int seatsPerRow)
        {
            try
            {
                var availableSeats = await _ticketService.GetAvailableSeatsForRow(sessionId, rowNumber, seatsPerRow);
                return Json(availableSeats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available seats");
                return Json(new List<int>());
            }
        }

        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogError("User ID claim not found or invalid");
                    return RedirectToAction("Login", "Account");
                }

                var tickets = await _ticketRepository.GetUserTicketsAsync(userId);
                if (tickets == null)
                {
                    _logger.LogWarning("No tickets found for user {UserId}", userId);
                    return View(Enumerable.Empty<Ticket>());
                }

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tickets for user");
                return View(Enumerable.Empty<Ticket>());
            }
        }
    }
}
