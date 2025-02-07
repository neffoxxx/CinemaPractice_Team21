using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Entities
{
    public class User
    {
        public User()
        {
            Username = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Role = "User";
            Tickets = new List<Ticket>();
        }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string PasswordHash { get; set; }

        public string Role { get; set; }
        
        public DateTime RegistrationDate { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
