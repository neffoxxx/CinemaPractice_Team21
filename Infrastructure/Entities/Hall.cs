using System.Collections.Generic;

namespace Infrastructure.Entities
{
    public class Hall
    {
        public int HallId { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public int RowsCount { get; set; }
        public int SeatsPerRow { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }

        // Навігаційні властивості
        public ICollection<Session> Sessions { get; set; }
    }
} 