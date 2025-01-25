using Infrastructure.Interfaces;

namespace Infrastructure.Entities
{
    public class Movie: IEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string TrailerUrl { get; set; }
        public float Rating { get; set; }
        public string PosterUrl { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; }
        public ICollection<MovieGenre> MovieGenres { get; set; }
        public ICollection<Session> Sessions { get; set; }
    }
}