namespace Infrastructure.Entities
{
    public class Genre
    {
        public Genre()
        {
            Name = string.Empty;
            MovieGenres = new List<MovieGenre>();
        }

        public int GenreId { get; set; }
        public required string Name { get; set; }
        public required ICollection<MovieGenre> MovieGenres { get; set; }
    }
}
