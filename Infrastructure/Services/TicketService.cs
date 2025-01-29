using Infrastructure.Repositories;

public interface ITicketService
{
    Task<bool> IsSeatAvailable(int sessionId, int rowNumber, int seatNumber);
    Task<(int Row, int Seat)> CalculateSeatPosition(int globalSeatNumber, int seatsPerRow);
    Task<List<int>> GetAvailableSeatsForRow(int sessionId, int rowNumber, int seatsPerRow);
    Task<List<int>> GetRowsWithAvailableSeats(int sessionId, int totalRows, int seatsPerRow);
}

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<bool> IsSeatAvailable(int sessionId, int rowNumber, int seatNumber)
    {
        var tickets = await _ticketRepository.GetTicketsBySessionAsync(sessionId);
        return !tickets.Any(t => t.RowNumber == rowNumber && t.SeatNumber == seatNumber.ToString());
    }

    public async Task<(int Row, int Seat)> CalculateSeatPosition(int globalSeatNumber, int seatsPerRow)
    {
        int rowNumber = ((globalSeatNumber - 1) / seatsPerRow) + 1;
        int seatInRow = ((globalSeatNumber - 1) % seatsPerRow) + 1;
        return (rowNumber, seatInRow);
    }

    public async Task<List<int>> GetAvailableSeatsForRow(int sessionId, int rowNumber, int seatsPerRow)
    {
        var tickets = await _ticketRepository.GetTicketsBySessionAsync(sessionId);
        var bookedSeatsInRow = tickets
            .Where(t => t.RowNumber == rowNumber)
            .Select(t => int.Parse(t.SeatNumber))
            .ToList();

        return Enumerable.Range(1, seatsPerRow)
            .Where(seat => !bookedSeatsInRow.Contains(seat))
            .ToList();
    }

    public async Task<List<int>> GetRowsWithAvailableSeats(int sessionId, int totalRows, int seatsPerRow)
    {
        var tickets = await _ticketRepository.GetTicketsBySessionAsync(sessionId);
        var availableRows = new List<int>();

        for (int row = 1; row <= totalRows; row++)
        {
            var bookedSeatsInRow = tickets
                .Where(t => t.RowNumber == row)
                .Select(t => int.Parse(t.SeatNumber))
                .ToList();

            if (bookedSeatsInRow.Count < seatsPerRow)
            {
                availableRows.Add(row);
            }
        }

        return availableRows;
    }
} 