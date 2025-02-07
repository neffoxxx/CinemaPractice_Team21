public class TicketBookingViewModel
{
    public required string SeatNumber { get; set; } = string.Empty;
    public required string MovieTitle { get; set; } = string.Empty;
    public required string HallName { get; set; } = string.Empty;
    
    public TicketBookingViewModel()
    {
        BookedSeats = new List<string>();
        AvailableRows = new List<int>();
    }

    public int SessionId { get; set; }
    public decimal Price { get; set; }
    public DateTime ShowTime { get; set; }
    public int HallId { get; set; }
    public int RowsCount { get; set; }
    public int SeatsPerRow { get; set; }
    public List<string> BookedSeats { get; set; } = new List<string>();
    public List<int> AvailableRows { get; set; } = new List<int>();
}
