using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppCore.DTOs;
using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Query;
using Infrastructure.Data;
using AppCore.ViewModels;
using AppCore.Interfaces;

namespace AppCore.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IHallRepository _hallRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SessionService> _logger;
        private readonly CinemaDbContext _context;

        public SessionService(
            ISessionRepository sessionRepository,
            IRepository<Movie> movieRepository,
            IHallRepository hallRepository,
            IMapper mapper,
            ILogger<SessionService> logger,
            CinemaDbContext context)
        {
            _sessionRepository = sessionRepository;
            _movieRepository = movieRepository;
            _hallRepository = hallRepository;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<SessionDTO>> GetAllSessionsAsync()
        {
            _logger.LogInformation("Getting all sessions with related data");
            var sessions = await _sessionRepository.GetAllWithIncludeAsync(
                query => query
                    .Include(s => s.Movie)
                    .Include(s => s.Hall) as IIncludableQueryable<Session, object>);

            if (sessions == null || !sessions.Any())
            {
                _logger.LogInformation("No sessions found");
                return new List<SessionDTO>();
            }

            // Фільтруємо сесії, залишаючи лише ті, у яких зал не null та активний
            sessions = sessions.Where(s => s.Hall != null && s.Hall.IsActive).ToList();

            // Сортуємо сесії за стартовим часом
            sessions = sessions.OrderBy(s => s.StartTime).ToList();

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

            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions);
            return sessionDtos;
        }

        public async Task<SessionDTO> GetSessionByIdAsync(int id)
        {
            _logger.LogInformation("Getting session by id: {Id}", id);
            var session = await _sessionRepository.GetByIdAsync(id);
            return _mapper.Map<SessionDTO>(session);
        }

        public async Task<SessionDTO> GetSessionByIdWithDetailsAsync(int id)
        {
            _logger.LogInformation("Getting session details by id: {Id}", id);
            var session = await _sessionRepository.GetByIdWithDetailsAsync(id) 
                          ?? throw new Exception($"Session not found with id: {id}");
            var sessionDto = _mapper.Map<SessionDTO>(session);
            // Використовуємо null-conditional оператор та задаємо значення за замовчуванням.
            sessionDto.MovieTitle = session.Movie?.Title ?? string.Empty;
            sessionDto.HallName = session.Hall?.Name ?? string.Empty;
            return sessionDto;
        }

        public async Task<SessionDTO?> GetSessionForEditAsync(int id)
        {
            _logger.LogInformation("Getting session for edit by id: {Id}", id);
            var session = await _sessionRepository.GetByIdWithDetailsAsync(id);
            return session == null ? null : _mapper.Map<SessionDTO>(session);
        }

        public async Task AddSessionAsync(SessionDTO sessionDto)
        {
            _logger.LogInformation("Adding new session for movie: {MovieId}", sessionDto.MovieId);

            // Перевіряємо наявність фільму.
            var movie = await _movieRepository.GetByIdAsync(sessionDto.MovieId);
            if (movie == null)
            {
                throw new Exception($"Movie not found with id: {sessionDto.MovieId}");
            }

            // Перевіряємо наявність залу.
            var hall = await _hallRepository.GetByIdAsync(sessionDto.HallId);
            if (hall == null)
            {
                throw new Exception($"Hall not found with id: {sessionDto.HallId}");
            }

            // Розраховуємо EndTime, використовуючи тривалість фільму.
            sessionDto.EndTime = sessionDto.StartTime.AddMinutes(movie!.DurationMinutes);

            // Перевіряємо доступність залу.
            var isHallAvailable = await _sessionRepository.IsHallAvailableAsync(
                sessionDto.HallId,
                sessionDto.StartTime,
                sessionDto.EndTime);

            if (!isHallAvailable)
            {
                throw new Exception("Hall is not available at the selected time");
            }

            var session = _mapper.Map<Session>(sessionDto);
            await _sessionRepository.AddAsync(session);
        }

        public async Task UpdateSessionAsync(SessionDTO sessionDto)
        {
            _logger.LogInformation("Updating session: {Id}", sessionDto.SessionId);
            
            // Отримуємо сесію і одразу перетворюємо її на ненульове значення, або кидаємо виключення.
            var session = await _sessionRepository.GetByIdAsync(sessionDto.SessionId)
                          ?? throw new Exception($"Session not found, ID: {sessionDto.SessionId}");
            
            // Оновлюємо об'єкт сесії за допомогою AutoMapper.
            _mapper.Map(sessionDto, session);

            // Отримуємо дані фільму і гарантуємо, що вони не null.
            var movie = await _movieRepository.GetByIdAsync(sessionDto.MovieId)
                        ?? throw new Exception($"Movie not found with id: {sessionDto.MovieId}");
            
            // Розраховуємо та встановлюємо EndTime.
            session.EndTime = sessionDto.StartTime.AddMinutes(movie.DurationMinutes);

            await _sessionRepository.UpdateAsync(session);
        }

        public async Task DeleteSessionAsync(int id)
        {
            _logger.LogInformation("Deleting session by id: {Id}", id);
            await _sessionRepository.DeleteAsync(id);
        }

        public async Task PopulateSessionSelectLists(SessionDTO model)
        {
            // Отримуємо всі фільми
            var movies = await _movieRepository.GetAllAsync();

            // Отримуємо зали і відфільтровуємо лише активні
            var halls = (await _hallRepository.GetAllAsync())
                            .Where(h => h.IsActive)
                            .ToList();

            // Якщо встановлено часові рамки (для редагування сесії),
            // додатково перевіряємо доступність зали
            if (model.StartTime != default(DateTime) && model.EndTime != default(DateTime))
            {
                var availableHalls = new List<Hall>();
                foreach (var hall in halls)
                {
                    if (await _sessionRepository.IsHallAvailableAsync(hall.HallId, model.StartTime, model.EndTime, model.SessionId))
                    {
                        availableHalls.Add(hall);
                    }
                }
                halls = availableHalls;
            }

            model.Movies = new SelectList(
                movies.Select(m => new SelectListItem { Value = m.MovieId.ToString(), Text = m.Title }),
                "Value", "Text");

            model.Halls = new SelectList(
                halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = h.Name }),
                "Value", "Text");
        }

        public async Task<IEnumerable<Session>> GetSessionsByFilmIdAsync(int filmId)
        {
            return await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Tickets)
                .Where(s => s.MovieId == filmId)
                .ToListAsync();
        }

        public async Task<IEnumerable<SessionViewModel>> GetSessionViewModelsByFilmIdAsync(int filmId)
        {
            var sessions = await GetSessionsByFilmIdAsync(filmId);
            return sessions.Select(session => new SessionViewModel
            {
                SessionId = session.SessionId,
                MovieId = session.MovieId,
                HallId = session.HallId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Price = session.Price,
                MovieTitle = session.Movie?.Title ?? string.Empty,
                HallName = session.Hall?.Name ?? string.Empty,
                SeatNumbers = session.Tickets?.Select(t => t.SeatNumber.ToString()).ToList() ?? new List<string>(),
                Capacity = session.Hall?.Capacity ?? 0
            });
        }

        public async Task<IEnumerable<SessionDTO>> GetFilteredSessionsAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            int? genreId, 
            decimal? minPrice, 
            decimal? maxPrice, 
            int? hallId, 
            string movieTitle)
        {
            _logger.LogInformation("Getting filtered sessions with parameters: StartDate={StartDate}, EndDate={EndDate}, GenreId={GenreId}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, HallId={HallId}, MovieTitle={MovieTitle}",
                startDate, endDate, genreId, minPrice, maxPrice, hallId, movieTitle);

            var sessions = await _sessionRepository.GetAllWithIncludeAsync(query => query
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Movie!.MovieGenres)
                .ThenInclude(mg => mg.Genre));

            // Додаємо фільтрацію – виключаємо сесії з неактивними залами
            sessions = sessions.Where(s => s.Hall != null && s.Hall.IsActive).ToList();

            // Застосування додаткових фільтрів, якщо вказано
            if (startDate.HasValue)
                sessions = sessions.Where(s => s.StartTime.Date >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                sessions = sessions.Where(s => s.EndTime.Date <= endDate.Value.Date).ToList();

            if (genreId.HasValue)
                sessions = sessions
                    .Where(s => s.Movie != null && s.Movie.MovieGenres.Any(mg => mg.GenreId == genreId.Value))
                    .ToList();

            if (minPrice.HasValue)
                sessions = sessions.Where(s => s.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                sessions = sessions.Where(s => s.Price <= maxPrice.Value).ToList();

            if (hallId.HasValue)
                sessions = sessions.Where(s => s.HallId == hallId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(movieTitle))
                sessions = sessions
                    .Where(s => s.Movie != null &&
                                s.Movie.Title.Contains(movieTitle, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();

            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions);

            foreach (var sessionDto in sessionDtos)
            {
                var sessionEntity = sessions.FirstOrDefault(s => s.SessionId == sessionDto.SessionId);

                if (sessionEntity != null)
                {
                    // Забезпечуємо ненульові рядкові значення.
                    sessionDto.MovieTitle = sessionEntity.Movie?.Title ?? string.Empty;
                    sessionDto.HallName = sessionEntity.Hall?.Name ?? string.Empty;
                }
            }

            return sessionDtos;
        }
    }
}