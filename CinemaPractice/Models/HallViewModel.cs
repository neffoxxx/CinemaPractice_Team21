using System.ComponentModel.DataAnnotations;

public class HallViewModel
{
    public int HallId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Rows count is required")]
    [Range(1, 50, ErrorMessage = "Rows count must be between 1 and 50")]
    public int RowsCount { get; set; }

    [Required(ErrorMessage = "Seats per row is required")]
    [Range(1, 50, ErrorMessage = "Seats per row must be between 1 and 50")]
    public int SeatsPerRow { get; set; }

    public bool IsActive { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string Description { get; set; }
} 