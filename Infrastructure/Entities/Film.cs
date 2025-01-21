namespace Infrastructure.Entities
{
    public class Film
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TrailerUrl { get; set; }
        public string Genre { get; set; }
        public string Cast { get; set; }
        public double Rating { get; set; }
    }

}
