using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        private readonly CinemaDbContext _context;

        public TicketRepository(CinemaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(int userId)
        {
            return await _context.Tickets
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Hall)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.BookingTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetAllWithDetailsAsync()
        {
            return await _context.Tickets
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.User)
                .OrderByDescending(t => t.BookingTime)
                .ToListAsync();
        }

        public async Task<Ticket> GetTicketBySeatAndSessionAsync(int sessionId, string seatNumber)
        {
            return await _context.Tickets
                .FirstOrDefaultAsync(t => 
                    t.SessionId == sessionId && 
                    t.SeatNumber == seatNumber.ToString() &&
                    t.Status == "Booked");
        }

        public async Task<List<string>> GetBookedSeatsForSessionAsync(int sessionId)
        {
            return await _context.Tickets
                .Where(t => t.SessionId == sessionId && t.Status == "Booked")
                .Select(t => t.SeatNumber)
                .ToListAsync();
        }

        public async Task<Ticket> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Hall)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }

        public async Task UpdateAsync(Ticket entity)
        {
            try
            {
                var existingTicket = await _context.Tickets.FindAsync(entity.TicketId);
                if (existingTicket != null)
                {
                    _context.Entry(existingTicket).CurrentValues.SetValues(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating ticket: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsBySessionAsync(int sessionId)
        {
            return await _context.Tickets
                .Where(t => t.SessionId == sessionId)
                .ToListAsync();
        }

        public Task<bool> AnyAsync(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
} 