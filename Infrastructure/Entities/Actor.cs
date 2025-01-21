namespace Infrastructure.Entities
{
    public class Actor
    {
        public int ActorId { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }

        public ICollection<MovieActor>? MovieActors { get; set; }
    }

}
