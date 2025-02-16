using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AppCore.DTOs
{
    public class MovieDTO : IValidatableObject
    {
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Director is required")]
        [StringLength(100, ErrorMessage = "Director cannot exceed 100 characters")]
        public required string Director { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public double Rating { get; set; }

        [Url(ErrorMessage = "Poster URL must be a valid URL")]
        public string PosterUrl { get; set; } = string.Empty;
        
        [Url(ErrorMessage = "Trailer URL must be a valid URL")]
        public string TrailerUrl { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0 minutes")]
        public int DurationMinutes { get; set; }
        
        public List<int> SelectedGenreIds { get; set; } = new();
        public List<int> SelectedActorIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedGenreIds == null || SelectedGenreIds.Count == 0)
            {
                yield return new ValidationResult(
                    "At least one genre must be selected",
                    new[] { nameof(SelectedGenreIds) }
                );
            }
        }
    }
}