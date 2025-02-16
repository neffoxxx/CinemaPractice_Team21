using System.ComponentModel.DataAnnotations;

namespace AppCore.DTOs
{
    public class MovieActorDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "MovieId must be greater than 0")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Movie title is required")]
        public string Title { get; set; } = string.Empty;

        public MovieDTO? Movie { get; set; }
    }
} 