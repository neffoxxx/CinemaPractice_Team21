using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Entities
{
    public class Session
    {
        public Session()
        {
            Tickets = new List<Ticket>();
        }

        public int SessionId { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
       
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
            
        public virtual Movie? Movie { get; set; }
        public virtual Hall? Hall { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
