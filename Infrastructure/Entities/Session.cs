namespace Infrastructure.Entities
{
    public class Session
    {
        public int SessionId { get; set; }
        public int MovieId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Hall { get; set; }
        public float Price { get; set; }

        public Movie Movie { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }

}
