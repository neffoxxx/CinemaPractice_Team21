using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Repositories
{
    public class SessionRepository : Repository<Session>, ISessionRepository
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<SessionRepository> _logger;

        public SessionRepository(CinemaDbContext context, ILogger<SessionRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<Session> GetByIdAsync(int id)
        {
            return await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.SessionId == id);
        }

        public async Task<Session> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.SessionId == id);
        }

        public async Task<IEnumerable<Session>> GetAllWithIncludeAsync(
            Func<IQueryable<Session>, IIncludableQueryable<Session, object>> include = null)
        {
            IQueryable<Session> query = _context.Sessions;

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Session>> GetFutureSessionsAsync()
        {
            return await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Where(s => s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsHallAvailableAsync(int hallId, DateTime startTime, DateTime endTime, int? excludeSessionId = null)
        {
            var query = _context.Sessions
                .Where(s => s.HallId == hallId &&
                           ((s.StartTime >= startTime && s.StartTime < endTime) ||
                            (s.EndTime > startTime && s.EndTime <= endTime) ||
                            (s.StartTime <= startTime && s.EndTime >= endTime)));

            if (excludeSessionId.HasValue)
            {
                query = query.Where(s => s.SessionId != excludeSessionId.Value);
            }

            return !await query.AnyAsync();
        }
    }
} 