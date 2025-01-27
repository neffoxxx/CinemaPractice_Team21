using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class SessionViewModel
{
    public int SessionId { get; set; }

    [Required]
    [Display(Name = "Movie")]
    public int MovieId { get; set; }

    [Required]
    [Display(Name = "Start Time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Display(Name = "End Time")]
    public DateTime EndTime { get; set; }

    [Required]
    [Display(Name = "Hall")]
    public string Hall { get; set; }

    [Required]
    [Display(Name = "Price")]
    [Range(0.01, float.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public float Price { get; set; }

    public SelectList? Movies { get; set; }
} 