using System.ComponentModel.DataAnnotations;

public class ActorDTO
{
    public int ActorId { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; }
    
    [StringLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters")]
    public string Bio { get; set; }
    
    public string PhotoUrl { get; set; }
}