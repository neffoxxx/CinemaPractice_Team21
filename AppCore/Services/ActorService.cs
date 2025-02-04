using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

public class ActorService : IActorService
{
    private readonly IRepository<Actor> _actorRepository;
    private readonly IMapper _mapper;

    public ActorService(IRepository<Actor> actorRepository, IMapper mapper)
    {
        _actorRepository = actorRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ActorDTO>> GetAllActorsAsync()
    {
        var actors = await _actorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ActorDTO>>(actors);
    }

    public async Task<ActorDTO> GetActorByIdAsync(int id)
    {
        var actor = await _actorRepository.GetByIdAsync(id);
        return _mapper.Map<ActorDTO>(actor);
    }

    public async Task AddActorAsync(ActorDTO actorDto)
    {
        var actor = _mapper.Map<Actor>(actorDto);
        await _actorRepository.AddAsync(actor);
    }

    public async Task UpdateActorAsync(ActorDTO actorDto)
    {
        var actor = _mapper.Map<Actor>(actorDto);
        await _actorRepository.UpdateAsync(actor);
    }

    public async Task DeleteActorAsync(int id)
    {
        await _actorRepository.DeleteAsync(id);
    }
}