using System;
using System.ComponentModel.DataAnnotations;

public class EditTicketViewModel
{
    public int TicketId { get; set; }
    public int SessionId { get; set; }
    public int UserId { get; set; }
    
    [Required(ErrorMessage = "Row number is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Row number must be greater than 0")]
    public int RowNumber { get; set; }
    
    [Required(ErrorMessage = "Seat number is required")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Seat number must be a positive integer")]
    public required string SeatNumber { get; set; }
    
    [Required(ErrorMessage = "Status is required")]
    public required string Status { get; set; }
    
    public DateTime BookingTime { get; set; }
    public required string MovieTitle { get; set; }
    public required string UserName { get; set; }
    public DateTime ShowTime { get; set; }
    public int HallId { get; set; }
    public required string HallName { get; set; }
} 