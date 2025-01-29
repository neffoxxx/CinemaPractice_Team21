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
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<AdminController> _logger;
        private readonly ITicketRepository _ticketRepository;
        private readonly IHallRepository _hallRepository;
        private readonly ITicketService _ticketService;

        public AdminController(
            IRepository<Movie> movieRepository, 
            ISessionRepository sessionRepository,
            ILogger<AdminController> logger,
            ITicketRepository ticketRepository,
            IHallRepository hallRepository,
            ITicketService ticketService)
        {
            _movieRepository = movieRepository;
            _sessionRepository = sessionRepository;
            _logger = logger;
            _ticketRepository = ticketRepository;
            _hallRepository = hallRepository;
            _ticketService = ticketService;
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
                Director = "",
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
                _logger.LogInformation("Loading sessions with related data");
                
                var sessions = await _sessionRepository.GetAllWithIncludeAsync(
                    query => query
                        .Include(s => s.Movie)
                        .Include(s => s.Hall)
                        .OrderBy(s => s.StartTime));

                if (sessions == null || !sessions.Any())
                {
                    _logger.LogInformation("No sessions found");
                    return View(new List<Session>());
                }

                // Перевірка завантаження даних
                foreach (var session in sessions)
                {
                    if (session.Movie == null)
                    {
                        _logger.LogWarning("Movie data missing for session {SessionId}", session.SessionId);
                    }
                    if (session.Hall == null)
                    {
                        _logger.LogWarning("Hall data missing for session {SessionId}", session.SessionId);
                    }
                }

                _logger.LogInformation("Successfully loaded {Count} sessions", sessions.Count());
                return View(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sessions");
                TempData["Error"] = "Error loading sessions: " + ex.Message;
                return View(new List<Session>());
            }
        }

        public async Task<IActionResult> AddSession()
        {
            try
            {
                _logger.LogInformation("Loading data for AddSession form");
                
                // Отримуємо дані
                var movies = (await _movieRepository.GetAllAsync()).ToList();
                var halls = (await _hallRepository.GetActiveHallsAsync()).ToList();

                _logger.LogInformation($"Found {movies.Count} movies and {halls.Count} halls");

                // Встановлюємо початковий час на наступну годину
                var startTime = DateTime.Now.AddHours(1);
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, 
                    startTime.Hour, 0, 0); // Округляємо до години

                var viewModel = new SessionViewModel
                {
                    StartTime = startTime,
                    EndTime = startTime, // EndTime буде розраховано на клієнтській стороні
                    Price = 100,
                    Movies = new SelectList(movies, nameof(Movie.MovieId), nameof(Movie.Title)),
                    Halls = new SelectList(halls, nameof(Hall.HallId), nameof(Hall.Name))
                };

                if (!movies.Any())
                {
                    ModelState.AddModelError("MovieId", "No movies available in the database");
                }
                if (!halls.Any())
                {
                    ModelState.AddModelError("HallId", "No halls available in the database");
                }

                return View(viewModel);
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
        public async Task<IActionResult> AddSession(SessionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Перезавантажуємо списки для представлення
                    var movies = await _movieRepository.GetAllAsync();
                    var halls = await _hallRepository.GetActiveHallsAsync();
                    model.Movies = new SelectList(movies, nameof(Movie.MovieId), nameof(Movie.Title));
                    model.Halls = new SelectList(halls, nameof(Hall.HallId), nameof(Hall.Name));
                    return View(model);
                }

                // Отримуємо фільм для перевірки тривалості
                var movie = await _movieRepository.GetByIdAsync(model.MovieId);
                if (movie == null)
                {
                    ModelState.AddModelError("MovieId", "Selected movie not found");
                    return View(model);
                }

                // Встановлюємо час закінчення на основі тривалості фільму
                model.EndTime = model.StartTime.AddMinutes(movie.DurationMinutes);

                var session = new Session
                {
                    MovieId = model.MovieId,
                    HallId = model.HallId,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Price = model.Price
                };

                await _sessionRepository.AddAsync(session);
                TempData["Success"] = "Session added successfully";
                return RedirectToAction(nameof(ManageSessions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding session");
                ModelState.AddModelError("", "Error adding session: " + ex.Message);
                return View(model);
            }
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
                MovieId = movie.MovieId,
                StartTime = DateTime.Now.AddDays(1),
                Price = 100
            };
            await _sessionRepository.AddAsync(session);

            return RedirectToAction(nameof(ManageFilms));
        }

        [HttpGet]
        public async Task<IActionResult> EditFilm(int id)
        {
            try
            {
                _logger.LogInformation("Loading film for editing, ID: {Id}", id);
                
                var movie = await _movieRepository.GetByIdAsync(id);
                if (movie == null)
                {
                    _logger.LogWarning("Film not found, ID: {Id}", id);
                    return NotFound();
                }

                _logger.LogInformation("Film loaded successfully: {Title}", movie.Title);
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
        public async Task<IActionResult> EditFilm(Movie movie)
        {
            try
            {
                _logger.LogInformation("Attempting to update film: {Id}", movie.MovieId);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid ModelState: {Errors}", string.Join(", ", errors));
                    return View(movie);
                }

                await _movieRepository.UpdateAsync(movie);
                
                _logger.LogInformation("Film updated successfully: {Id}", movie.MovieId);
                TempData["Success"] = "Film updated successfully";
                return RedirectToAction(nameof(ManageFilms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating film: {Id}", movie.MovieId);
                ModelState.AddModelError("", "Error updating film: " + ex.Message);
                return View(movie);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditSession(int id)
        {
            try
            {
                var session = await _sessionRepository.GetByIdWithDetailsAsync(id);
                if (session == null)
                {
                    return NotFound();
                }

                var movies = await _movieRepository.GetAllAsync();
                var halls = await _hallRepository.GetActiveHallsAsync();

                var viewModel = new SessionViewModel
                {
                    SessionId = session.SessionId,
                    MovieId = session.MovieId,
                    HallId = session.HallId,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    Price = session.Price,
                    Movies = new SelectList(movies, nameof(Movie.MovieId), nameof(Movie.Title), session.MovieId),
                    Halls = new SelectList(halls, nameof(Hall.HallId), nameof(Hall.Name), session.HallId)
                };

                return View(viewModel);
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
        public async Task<IActionResult> EditSession(SessionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var movies = await _movieRepository.GetAllAsync();
                    var halls = await _hallRepository.GetActiveHallsAsync();
                    model.Movies = new SelectList(movies, nameof(Movie.MovieId), nameof(Movie.Title), model.MovieId);
                    model.Halls = new SelectList(halls, nameof(Hall.HallId), nameof(Hall.Name), model.HallId);
                    return View(model);
                }

                // Отримуємо фільм для перевірки тривалості
                var movie = await _movieRepository.GetByIdAsync(model.MovieId);
                if (movie == null)
                {
                    ModelState.AddModelError("MovieId", "Selected movie not found");
                    return View(model);
                }

                // Розраховуємо час закінчення на основі тривалості фільму
                var calculatedEndTime = model.StartTime.AddMinutes(movie.DurationMinutes);

                var session = await _sessionRepository.GetByIdAsync(model.SessionId);
                if (session == null)
                {
                    return NotFound();
                }

                session.MovieId = model.MovieId;
                session.HallId = model.HallId;
                session.StartTime = model.StartTime;
                session.EndTime = calculatedEndTime;
                session.Price = model.Price;

                await _sessionRepository.UpdateAsync(session);
                TempData["Success"] = "Session updated successfully";
                return RedirectToAction(nameof(ManageSessions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session");
                ModelState.AddModelError("", "Error updating session: " + ex.Message);
                
                var movies = await _movieRepository.GetAllAsync();
                var halls = await _hallRepository.GetActiveHallsAsync();
                model.Movies = new SelectList(movies, nameof(Movie.MovieId), nameof(Movie.Title), model.MovieId);
                model.Halls = new SelectList(halls, nameof(Hall.HallId), nameof(Hall.Name), model.HallId);
                
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageTickets()
        {
            var tickets = await _ticketRepository.GetAllWithDetailsAsync();
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> EditTicket(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);
            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("ManageTickets");
            }

            var model = new EditTicketViewModel
            {
                TicketId = ticket.TicketId,
                SessionId = ticket.SessionId,
                UserId = ticket.UserId,
                RowNumber = ticket.RowNumber,
                SeatNumber = ticket.SeatNumber,
                Status = ticket.Status,
                BookingTime = ticket.BookingTime,
                MovieTitle = ticket.Session?.Movie?.Title,
                UserName = ticket.User?.Username,
                ShowTime = ticket.Session?.StartTime ?? DateTime.MinValue,
                HallId = ticket.Session?.HallId ?? 0,
                HallName = ticket.Session?.Hall?.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTicket(EditTicketViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var ticket = await _ticketRepository.GetByIdAsync(model.TicketId);
                if (ticket == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction("ManageTickets");
                }

                // Перевіряємо, чи змінилося місце
                if (ticket.RowNumber != model.RowNumber || ticket.SeatNumber != model.SeatNumber)
                {
                    // Перевіряємо, чи нове місце доступне
                    var isSeatAvailable = await _ticketService.IsSeatAvailable(
                        ticket.SessionId, 
                        model.RowNumber, 
                        int.Parse(model.SeatNumber));

                    if (!isSeatAvailable)
                    {
                        ModelState.AddModelError("", "This seat is already booked. Please select another seat.");
                        return View(model);
                    }
                }

                // Валідація номера ряду та місця
                var session = await _sessionRepository.GetByIdWithDetailsAsync(ticket.SessionId);
                if (session == null)
                {
                    ModelState.AddModelError("", "Session not found.");
                    return View(model);
                }

                if (model.RowNumber <= 0 || model.RowNumber > session.Hall.RowsCount)
                {
                    ModelState.AddModelError(nameof(model.RowNumber), 
                        $"Row number must be between 1 and {session.Hall.RowsCount}.");
                    return View(model);
                }

                var seatNumber = int.Parse(model.SeatNumber);
                if (seatNumber <= 0 || seatNumber > session.Hall.SeatsPerRow)
                {
                    ModelState.AddModelError(nameof(model.SeatNumber), 
                        $"Seat number must be between 1 and {session.Hall.SeatsPerRow}.");
                    return View(model);
                }

                // Оновлюємо дані квитка
                ticket.RowNumber = model.RowNumber;
                ticket.SeatNumber = model.SeatNumber;
                ticket.Status = model.Status;

                await _ticketRepository.UpdateAsync(ticket);
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
            await _ticketRepository.DeleteAsync(id);
            return RedirectToAction(nameof(ManageTickets));
        }

        public async Task<IActionResult> ManageHalls()
        {
            var halls = await _hallRepository.GetAllAsync();
            var viewModels = halls.Select(h => new HallViewModel
            {
                HallId = h.HallId,
                Name = h.Name,
                Capacity = h.Capacity,
                RowsCount = h.RowsCount,
                SeatsPerRow = h.SeatsPerRow,
                IsActive = h.IsActive,
                Description = h.Description
            });
            return View(viewModels);
        }

        public IActionResult AddHall()
        {
            return View(new HallViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHall(HallViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var hall = new Hall
                {
                    Name = model.Name,
                    Capacity = model.Capacity,
                    RowsCount = model.RowsCount,
                    SeatsPerRow = model.SeatsPerRow,
                    IsActive = model.IsActive,
                    Description = model.Description
                };

                await _hallRepository.AddAsync(hall);
                _logger.LogInformation("Hall {HallName} created successfully", hall.Name);
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
            var hall = await _hallRepository.GetByIdAsync(id);
            if (hall == null)
                return NotFound();

            var viewModel = new HallViewModel
            {
                HallId = hall.HallId,
                Name = hall.Name,
                Capacity = hall.Capacity,
                RowsCount = hall.RowsCount,
                SeatsPerRow = hall.SeatsPerRow,
                IsActive = hall.IsActive,
                Description = hall.Description
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHall(HallViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var hall = await _hallRepository.GetByIdAsync(model.HallId);
                if (hall == null)
                    return NotFound();

                hall.Name = model.Name;
                hall.Capacity = model.Capacity;
                hall.RowsCount = model.RowsCount;
                hall.SeatsPerRow = model.SeatsPerRow;
                hall.IsActive = model.IsActive;
                hall.Description = model.Description;

                await _hallRepository.UpdateAsync(hall);
                _logger.LogInformation("Hall {HallId} updated successfully", hall.HallId);
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
                var hall = await _hallRepository.GetByIdAsync(id);
                if (hall == null)
                    return NotFound();

                await _hallRepository.DeleteAsync(id);
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
