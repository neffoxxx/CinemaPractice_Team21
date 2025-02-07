using System.ComponentModel.DataAnnotations;

public class HallViewModel
{
    public int HallId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }

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

    [Required(ErrorMessage = "Description is required")]
    public required string Description { get; set; } = string.Empty;
} 