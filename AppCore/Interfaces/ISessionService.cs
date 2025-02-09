using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Entities;
using AppCore.DTOs;
using AppCore.ViewModels;
using System;

namespace AppCore.Interfaces
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionDTO>> GetAllSessionsAsync();
        Task<SessionDTO> GetSessionByIdAsync(int id);
        Task<SessionDTO> GetSessionByIdWithDetailsAsync(int id);
        Task<SessionDTO?> GetSessionForEditAsync(int id);
        Task AddSessionAsync(SessionDTO sessionDto);
        Task UpdateSessionAsync(SessionDTO sessionDto);
        Task DeleteSessionAsync(int id);
        Task PopulateSessionSelectLists(SessionDTO model);
        Task<IEnumerable<Session>> GetSessionsByFilmIdAsync(int filmId);
        Task<IEnumerable<SessionViewModel>> GetSessionViewModelsByFilmIdAsync(int filmId);
        Task<IEnumerable<SessionDTO>> GetFilteredSessionsAsync(
            DateTime? startDate,
            DateTime? endDate,
            int? genreId,
            decimal? minPrice,
            decimal? maxPrice,
            int? hallId,
            string movieTitle);
    }
}
