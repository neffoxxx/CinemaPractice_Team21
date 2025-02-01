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
        public string UserId { get; set; }

        [Required(ErrorMessage = "Row number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Row number must be greater than 0")]
        public int RowNumber { get; set; }

        [Required(ErrorMessage = "Seat number is required")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Seat number must be numeric")]
        public string SeatNumber { get; set; }

        public DateTime BookingTime { get; set; }

        public string Status { get; set; }

        public string MovieTitle { get; set; }
        public string UserName { get; set; }
        public DateTime ShowTime { get; set; }
        public int HallId { get; set; }
        public string HallName { get; set; }
    }
}