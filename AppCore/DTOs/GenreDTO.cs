using System.ComponentModel.DataAnnotations;

namespace AppCore.DTOs
{
    public class GenreDTO
    {
        public int GenreId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public required string Name { get; set; }
    }
}