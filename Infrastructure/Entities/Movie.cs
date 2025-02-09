using System.ComponentModel.DataAnnotations;
using Infrastructure.Interfaces;

namespace Infrastructure.Entities
{
    public class Movie : IEntity
    {
        public Movie()
        {
            Title = string.Empty;
            Description = string.Empty;
            Director = string.Empty;
            PosterUrl = string.Empty;
            TrailerUrl = string.Empty;
            MovieActors = new List<MovieActor>();
            MovieGenres = new List<MovieGenre>();
            Sessions = new List<Session>();
        }

        public int MovieId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Director is required")]
        public required string Director { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public double Rating { get; set; }

        public required string PosterUrl { get; set; }
        public required string TrailerUrl { get; set; }
        public int DurationMinutes { get; set; }

        // Позначаємо колекції як віртуальні та необов'язкові
        public virtual ICollection<MovieActor> MovieActors { get; set; }
        public virtual ICollection<MovieGenre> MovieGenres { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }

        // Реалізація інтерфейсу IEntity
        int IEntity.Id 
        {
            get => MovieId;
            set => MovieId = value;
        }
    }
}