namespace Infrastructure.Entities
{
    public class Ticket
    {
        public Ticket()
        {
            SeatNumber = string.Empty;
            Status = "Pending";
        }

        public int TicketId { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public int RowNumber { get; set; }
        public required string SeatNumber { get; set; }
        public DateTime BookingTime { get; set; }
        public required string Status { get; set; }
        public virtual Session? Session { get; set; }
        public virtual User? User { get; set; }
    }
}
