using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Entities
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string firstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string lastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }

        public string Role { get; set; }
        
        public DateTime RegistrationDate { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
