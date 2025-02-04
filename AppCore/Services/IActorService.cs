public interface IActorService
{
    Task<IEnumerable<ActorDTO>> GetAllActorsAsync();
    Task<ActorDTO> GetActorByIdAsync(int id);
    Task AddActorAsync(ActorDTO actorDto);
    Task UpdateActorAsync(ActorDTO actorDto);
    Task DeleteActorAsync(int id);
}