﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AppCore.DTOs
{
    public class MovieDTO
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
        public required string Director { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public double Rating { get; set; }

        public string PosterUrl { get; set; } = string.Empty;
        public string TrailerUrl { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new();
        public List<int> SelectedActorIds { get; set; } = new();
    }
}