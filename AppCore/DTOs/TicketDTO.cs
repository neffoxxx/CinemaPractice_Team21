using System;
using System.ComponentModel.DataAnnotations;

namespace AppCore.DTOs
{
    public class TicketDTO
    {
        public int TicketId { get; set; }
        public int SessionId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime BookingTime { get; set; }
        
        // Додайте наступні властивості
        public int HallId { get; set; }
        public string HallName { get; set; } = string.Empty;
        
        // Інші властивості...
        public string MovieTitle { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ShowTime { get; set; }
        public int RowNumber { get; set; }
        public bool IsHallActive { get; set; }

        public string HallStatusText => !IsHallActive ? "Active" : "Inactive";
    }
}