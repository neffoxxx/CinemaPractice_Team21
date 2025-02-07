using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Infrastructure.Entities;

namespace AppCore.ViewModels
{
    public class SessionViewModel
    {
        public int SessionId { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public string? MovieTitle { get; set; }
        public string? HallName { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public int Capacity { get; set; }
    }
} 