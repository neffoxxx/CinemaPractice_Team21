using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Infrastructure.Entities;

public class SessionViewModel
{
    public int SessionId { get; set; }

    [Required(ErrorMessage = "Please select a movie")]
    [Display(Name = "Movie")]
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Please select a hall")]
    [Display(Name = "Hall")]
    public int HallId { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    [Display(Name = "Start Time")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "End time is required")]
    [Display(Name = "End Time")]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; } = DateTime.Now.AddHours(2);

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000, ErrorMessage = "Price must be between 0.01 and 1000")]
    [Display(Name = "Price")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; } = 100;

    public SelectList Movies { get; set; } = new SelectList(Enumerable.Empty<Movie>());
    public SelectList Halls { get; set; } = new SelectList(Enumerable.Empty<Hall>());
} 