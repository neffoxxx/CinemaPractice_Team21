using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

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
                .FirstOrDefaultAsync(s => s.SessionId == id);
        }

        public async Task<IEnumerable<Session>> GetAllWithIncludeAsync(
            Func<IQueryable<Session>, IQueryable<Session>> include)
        {
            try
            {
                var query = _context.Sessions.AsQueryable();
                query = include(query);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions with includes");
                throw;
            }
        }
    }
} 