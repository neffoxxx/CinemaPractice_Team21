using AppCore.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDTO>> GetAllTicketsAsync();
        Task<TicketDTO?> GetTicketByIdAsync(int id);
        Task UpdateTicketAsync(TicketDTO ticketDto);
        Task DeleteTicketAsync(int id);
        Task<bool> IsSeatAvailable(int sessionId, int rowNumber, int seatNumber);
        Task<(int Row, int Seat)> CalculateSeatPosition(int globalSeatNumber, int seatsPerRow);
        Task<List<int>> GetAvailableSeatsForRow(int sessionId, int rowNumber, int seatsPerRow);
        Task<List<int>> GetRowsWithAvailableSeats(int sessionId, int totalRows, int seatsPerRow);
    }
}