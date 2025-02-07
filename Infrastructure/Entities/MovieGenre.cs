namespace Infrastructure.Entities
{
    public class MovieGenre
    {
        public MovieGenre()
        {
            Movie = null!;
            Genre = null!;
        }

        public int MovieId { get; set; }
        public int GenreId { get; set; }

        public virtual required Movie Movie { get; set; }
        public virtual required Genre Genre { get; set; }
    }
}
