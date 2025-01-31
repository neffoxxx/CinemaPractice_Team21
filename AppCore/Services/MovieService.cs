using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AppCore.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AppCore.Services
{
    public class MovieService : IMovieService
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IRepository<Movie> movieRepository, IMapper mapper, ILogger<MovieService> logger)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<MovieDTO>> GetAllMoviesAsync()
        {
            _logger.LogInformation("Getting all movies");
            var movies = await _movieRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MovieDTO>>(movies);
        }

        public async Task<MovieDTO> GetMovieByIdAsync(int id)
        {
            _logger.LogInformation("Getting movie by id: {Id}", id);
            var movie = await _movieRepository.GetByIdAsync(id);
            return _mapper.Map<MovieDTO>(movie);
        }

        public async Task AddMovieAsync(MovieDTO movieDto)
        {
            _logger.LogInformation("Adding new movie: {Title}", movieDto.Title);
            var movie = _mapper.Map<Movie>(movieDto);
            await _movieRepository.AddAsync(movie);
        }

        public async Task UpdateMovieAsync(MovieDTO movieDto)
        {
            _logger.LogInformation("Updating movie: {Id}", movieDto.MovieId);
            var movie = _mapper.Map<Movie>(movieDto);
            await _movieRepository.UpdateAsync(movie);
        }

        public async Task DeleteMovieAsync(int id)
        {
            _logger.LogInformation("Deleting movie by id: {Id}", id);
            await _movieRepository.DeleteAsync(id);
        }
    }

    public interface IMovieService
    {
        Task<IEnumerable<MovieDTO>> GetAllMoviesAsync();
        Task<MovieDTO> GetMovieByIdAsync(int id);
        Task AddMovieAsync(MovieDTO movieDto);
        Task UpdateMovieAsync(MovieDTO movieDto);
        Task DeleteMovieAsync(int id);
    }
}