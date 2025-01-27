using System.ComponentModel.DataAnnotations;
using Infrastructure.Interfaces;

namespace Infrastructure.Entities
{
    public class Movie : IEntity
    {
        public Movie()
        {
            // Ініціалізуємо колекції в конструкторі
            MovieActors = new List<MovieActor>();
            MovieGenres = new List<MovieGenre>();
            Sessions = new List<Session>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 1000, ErrorMessage = "Duration must be between 1 and 1000 minutes")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        public DateTime ReleaseDate { get; set; }

        public string TrailerUrl { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public float Rating { get; set; }

        public string PosterUrl { get; set; }

        // Позначаємо колекції як віртуальні та необов'язкові
        public virtual ICollection<MovieActor>? MovieActors { get; set; }
        public virtual ICollection<MovieGenre>? MovieGenres { get; set; }
        public virtual ICollection<Session>? Sessions { get; set; }
    }
}