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

namespace AppCore.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IHallRepository _hallRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            ISessionRepository sessionRepository,
            IRepository<Movie> movieRepository,
            IHallRepository hallRepository,
            IMapper mapper,
            ILogger<SessionService> logger)
        {
            _sessionRepository = sessionRepository;
            _movieRepository = movieRepository;
            _hallRepository = hallRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<SessionDTO>> GetAllSessionsAsync()
        {
            _logger.LogInformation("Getting all sessions with related data");
            var sessions = await _sessionRepository.GetAllWithIncludeAsync(
                   query => query
                       .Include(s => s.Movie)
                       .Include(s => s.Hall));

            if (sessions == null || !sessions.Any())
            {
                _logger.LogInformation("No sessions found");
                return new List<SessionDTO>();
            }

            // Сортуємо результати після отримання даних
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
            var session = await _sessionRepository.GetByIdWithDetailsAsync(id);
            var sessionDto = _mapper.Map<SessionDTO>(session);
            sessionDto.MovieTitle = session.Movie?.Title;
            sessionDto.HallName = session.Hall?.Name;
            return sessionDto;
        }

        public async Task<SessionDTO> GetSessionForEditAsync(int id)
        {
            _logger.LogInformation("Getting session for edit by id: {Id}", id);
            var session = await _sessionRepository.GetByIdWithDetailsAsync(id);
            if (session == null)
            {
                _logger.LogWarning("Session not found, ID: {Id}", id);
                return null;
            }

            var movies = await _movieRepository.GetAllAsync();
            var halls = await _hallRepository.GetActiveHallsAsync();

            var viewModel = _mapper.Map<SessionDTO>(session);
            viewModel.Movies = new SelectList(
                 movies.Select(m => new SelectListItem { Value = m.MovieId.ToString(), Text = m.Title }),
                  "Value", "Text", session.MovieId.ToString());
            viewModel.Halls = new SelectList(
                halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = h.Name }),
                 "Value", "Text", session.HallId.ToString());

            return viewModel;
        }

        public async Task AddSessionAsync(SessionDTO sessionDto)
        {
            _logger.LogInformation("Adding new session for movie: {MovieId}", sessionDto.MovieId);

            // Перевірка наявності фільму
            var movie = await _movieRepository.GetByIdAsync(sessionDto.MovieId);
            if (movie == null)
            {
                throw new Exception($"Movie not found with id: {sessionDto.MovieId}");
            }

            // Перевірка наявності залу
            var hall = await _hallRepository.GetByIdAsync(sessionDto.HallId);
            if (hall == null)
            {
                throw new Exception($"Hall not found with id: {sessionDto.HallId}");
            }

            // Розрахунок часу закінчення сеансу
            sessionDto.EndTime = sessionDto.StartTime.AddMinutes(movie.DurationMinutes);

            // Перевірка доступності залу
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
            var session = await _sessionRepository.GetByIdAsync(sessionDto.SessionId);

            if (session == null)
            {
                _logger.LogError("Session not found, ID: {Id}", sessionDto.SessionId);
                throw new Exception($"Session not found, ID: {sessionDto.SessionId}");
            }

            // Update session object
            _mapper.Map(sessionDto, session);

            // Get movie for the duration
            var movie = await _movieRepository.GetByIdAsync(sessionDto.MovieId);
            if (movie == null)
            {
                _logger.LogError("Movie not found with id: {MovieId}", sessionDto.MovieId);
                throw new Exception($"Movie not found with id: {sessionDto.MovieId}");
            }
            // Calculate and set EndTime for the session
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
            var movies = await _movieRepository.GetAllAsync();
            var halls = await _hallRepository.GetActiveHallsAsync();
            model.Movies = new SelectList(
               movies.Select(m => new SelectListItem { Value = m.MovieId.ToString(), Text = m.Title }),
               "Value", "Text");
            model.Halls = new SelectList(
                 halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = h.Name }),
                "Value", "Text");
        }

        public async Task<IEnumerable<SessionDTO>> GetFilteredSessionsAsync(DateTime? startDate, DateTime? endDate, int? genreId)
        {
            _logger.LogInformation("Getting filtered sessions with parameters: StartDate={StartDate}, EndDate={EndDate}, GenreId={GenreId}", startDate, endDate, genreId);

            var sessions = await _sessionRepository.GetAllWithIncludeAsync(query => query
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Movie.MovieGenres)
                .ThenInclude(mg => mg.Genre));


            // Apply filters using LINQ to Objects
            if (startDate.HasValue)
            {
                sessions = sessions.Where(s => s.StartTime.Date >= startDate.Value.Date).ToList();
            }

            if (endDate.HasValue)
            {
                sessions = sessions.Where(s => s.EndTime.Date <= endDate.Value.Date).ToList();
            }

            if (genreId.HasValue)
            {
                sessions = sessions.Where(s => s.Movie.MovieGenres.Any(mg => mg.GenreId == genreId.Value)).ToList();
            }

            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions);

            foreach (var sessionDto in sessionDtos)
            {
                var sessionEntity = sessions.FirstOrDefault(s => s.SessionId == sessionDto.SessionId);

                if (sessionEntity != null)
                {
                    sessionDto.MovieTitle = sessionEntity.Movie?.Title;
                    sessionDto.HallName = sessionEntity.Hall?.Name;
                }
            }

            return sessionDtos;
        }
    }
}

public interface ISessionService
    {
        Task<IEnumerable<SessionDTO>> GetAllSessionsAsync();
        Task<SessionDTO> GetSessionByIdAsync(int id);
        Task<SessionDTO> GetSessionByIdWithDetailsAsync(int id);
        Task<SessionDTO> GetSessionForEditAsync(int id);
        Task AddSessionAsync(SessionDTO sessionDto);
        Task UpdateSessionAsync(SessionDTO sessionDto);
        Task DeleteSessionAsync(int id);
        Task PopulateSessionSelectLists(SessionDTO model);
         Task<IEnumerable<SessionDTO>> GetFilteredSessionsAsync(DateTime? startDate, DateTime? endDate, int? genreId);
}