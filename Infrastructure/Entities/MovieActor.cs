namespace Infrastructure.Entities
{
    public class MovieActor
    {
        public MovieActor()
        {
            Movie = null!;
            Actor = null!;
        }

        public int MovieId { get; set; }
        public int ActorId { get; set; }

        public virtual required Movie Movie { get; set; }
        public virtual required Actor Actor { get; set; }
    }
}
