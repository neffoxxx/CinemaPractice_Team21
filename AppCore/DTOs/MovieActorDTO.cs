namespace AppCore.DTOs
{
    public class MovieActorDTO
    {
        public int MovieId { get; set; } // Ідентифікатор фільму
        public string Title { get; set; } = string.Empty; // Назва фільму
        public MovieDTO? Movie { get; set; } // Якщо у вас є MovieDTO
    }
} 