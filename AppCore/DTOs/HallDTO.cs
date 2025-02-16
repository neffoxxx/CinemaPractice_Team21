using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppCore.DTOs
{
    public class HallDTO
    {
        public int HallId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Rows count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Rows count must be greater than 0")]
        public int RowsCount { get; set; }

        [Required(ErrorMessage = "Seats per row is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Seats per row must be greater than 0")]
        public int SeatsPerRow { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}