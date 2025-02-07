using System;
using System.ComponentModel.DataAnnotations;

namespace AppCore.DTOs
{
    public class TicketDTO
    {
        public int TicketId { get; set; }
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "User is required")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Row number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Row number must be greater than 0")]
        public int RowNumber { get; set; }

        [Required(ErrorMessage = "Seat number is required")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Seat number must be numeric")]
        public required string SeatNumber { get; set; }

        public DateTime BookingTime { get; set; }

        public required string Status { get; set; }

        public required string MovieTitle { get; set; }
        public required string UserName { get; set; }
        public DateTime ShowTime { get; set; }
        public int HallId { get; set; }
        public required string HallName { get; set; }
    }
}