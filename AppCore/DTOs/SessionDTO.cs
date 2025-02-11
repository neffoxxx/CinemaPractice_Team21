using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppCore.DTOs
{
    public class SessionDTO
    {
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Movie is required")]
        [Range(1, int.MaxValue, ErrorMessage = "MovieId must be greater than 0")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Hall is required")]
        [Range(1, int.MaxValue, ErrorMessage = "HallId must be greater than 0")]
        public int HallId { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        // Navigation properties
        public string? MovieTitle { get; set; }
        public string? HallName { get; set; }

        // SelectLists for dropdowns
        public SelectList? Movies { get; set; }
        public SelectList? Halls { get; set; }
    }
}