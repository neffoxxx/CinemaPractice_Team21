using Infrastructure.Entities;
using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        new Task<Session?> GetByIdAsync(int id);
        Task<Session?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Session>> GetAllWithIncludeAsync(
            Func<IQueryable<Session>, IIncludableQueryable<Session, object>>? include = null);
        Task<IEnumerable<Session>> GetFutureSessionsAsync();
        Task<bool> IsHallAvailableAsync(int hallId, DateTime startTime, DateTime endTime, int currentSessionId = 0);
    }
} 