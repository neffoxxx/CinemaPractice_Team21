using System;
using System.ComponentModel.DataAnnotations;

namespace AppCore.DTOs
{
    public class TicketDTO
    {
        public int TicketId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "SessionId must be greater than 0")]
        public int SessionId { get; set; }
        
        [Required(ErrorMessage = "Seat number is required")]
        public string SeatNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
        
        public DateTime BookingTime { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "HallId must be greater than 0")]
        public int HallId { get; set; }
        
        [Required(ErrorMessage = "Hall name is required")]
        public string HallName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Movie title is required")]
        public string MovieTitle { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; } = string.Empty;
        
        public DateTime ShowTime { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Row number must be greater than 0")]
        public int RowNumber { get; set; }
        
        public bool IsHallActive { get; set; }

        public string HallStatusText => !IsHallActive ? "Active" : "Inactive";
    }
}