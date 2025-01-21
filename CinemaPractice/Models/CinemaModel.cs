namespace CinemaPractice.Models
{
    public class CinemaModel
    {
        // Properties
        public int Id { get; set; } // Unique identifier for the cinema
        public string Name { get; set; } // Name of the cinema
        public string Location { get; set; } // Address or location of the cinema
        public int NumberOfHalls { get; set; } // Number of screening halls in the cinema
        public bool IsOpen { get; set; } // Status indicating whether the cinema is open

        // Constructor
        public CinemaModel(int id, string name, string location, int numberOfHalls, bool isOpen)
        {
            Id = id;
            Name = name;
            Location = location;
            NumberOfHalls = numberOfHalls;
            IsOpen = isOpen;
        }

        // Methods
        public void OpenCinema()
        {
            IsOpen = true;
        }

        public void CloseCinema()
        {
            IsOpen = false;
        }

        public override string ToString()
        {
            return $"Cinema: {Name}, Location: {Location}, Halls: {NumberOfHalls}, Status: {(IsOpen ? "Open" : "Closed")}";
        }
    }
}