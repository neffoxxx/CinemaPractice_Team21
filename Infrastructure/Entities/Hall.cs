using System.Collections.Generic;

namespace Infrastructure.Entities
{
    public class Hall
    {
        public Hall()
        {
            Name = string.Empty;
            Description = string.Empty;
            Sessions = new List<Session>();
        }

        public int HallId { get; set; }
        public required string Name { get; set; }
        public int Capacity { get; set; }
        public int RowsCount { get; set; }
        public int SeatsPerRow { get; set; }
        public bool IsActive { get; set; }
        public required string Description { get; set; }

        // Навігаційні властивості
        public required ICollection<Session> Sessions { get; set; }

        // Нова властивість для перевірки, чи можна бронювати місця
        public bool CanBookSeats => IsActive;
    }
} 