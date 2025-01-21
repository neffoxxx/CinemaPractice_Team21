namespace Infrastructure.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }

}
