using Infrastructure.Entities;
using Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetUserTicketsAsync(int userId);
        Task<IEnumerable<Ticket>> GetAllWithDetailsAsync();
        Task<Ticket> GetTicketBySeatAndSessionAsync(int sessionId, string seatNumber);
        Task<List<string>> GetBookedSeatsForSessionAsync(int sessionId);
        Task<Ticket> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsBySessionAsync(int sessionId);
        Task<bool> AnyAsync(Func<object, bool> value);
    }
} 