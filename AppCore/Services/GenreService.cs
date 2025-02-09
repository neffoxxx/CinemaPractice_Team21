using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using AppCore.DTOs;
using AppCore.Interfaces;
public class GenreService : IGenreService
{
    private readonly IRepository<Genre> _genreRepository;
    private readonly IMapper _mapper;

    public GenreService(IRepository<Genre> genreRepository, IMapper mapper)
    {
        _genreRepository = genreRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GenreDTO>> GetAllGenresAsync()
    {
        var genres = await _genreRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<GenreDTO>>(genres);
    }

    public async Task<GenreDTO> GetGenreByIdAsync(int id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        return _mapper.Map<GenreDTO>(genre);
    }

    public async Task AddGenreAsync(GenreDTO genreDto)
    {
        var genre = _mapper.Map<Genre>(genreDto);
        await _genreRepository.AddAsync(genre);
    }

    public async Task UpdateGenreAsync(GenreDTO genreDto)
    {
        var genre = _mapper.Map<Genre>(genreDto);
        await _genreRepository.UpdateAsync(genre);
    }

    public async Task DeleteGenreAsync(int id)
    {
        await _genreRepository.DeleteAsync(id);
    }
}