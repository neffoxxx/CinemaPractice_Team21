using AppCore.DTOs;
using AutoMapper;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ActorService : IActorService
{
    private readonly IRepository<Actor> _actorRepository;
    private readonly IMapper _mapper;
    private readonly CinemaDbContext _context;

    public ActorService(IRepository<Actor> actorRepository, IMapper mapper, CinemaDbContext context)
    {
        _actorRepository = actorRepository;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<ActorDTO>> GetAllActorsAsync()
    {
        var actors = await _actorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ActorDTO>>(actors);
    }

    public async Task<ActorDTO?> GetActorByIdAsync(int id)
    {
        var actor = await _context.Actors
            .Include(a => a.MovieActors!)
            .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.ActorId == id);

        if (actor == null) return null;

        return new ActorDTO
        {
            ActorId = actor.ActorId,
            Name = actor.Name ?? string.Empty,
            Bio = actor.Bio ?? string.Empty,
            PhotoUrl = actor.PhotoUrl ?? string.Empty,
            MovieActors = actor.MovieActors?.Select(ma => new MovieActorDTO
            {
                MovieId = ma.Movie?.MovieId ?? 0,
                Title = ma.Movie?.Title ?? string.Empty
            }).ToList() ?? new List<MovieActorDTO>()
        };
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