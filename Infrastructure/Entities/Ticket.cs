namespace Infrastructure.Entities
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public int RowNumber { get; set; }
        public string SeatNumber { get; set; }
        public DateTime BookingTime { get; set; }
        public string Status { get; set; }

        public Session Session { get; set; }
        public User User { get; set; }
    }

}
