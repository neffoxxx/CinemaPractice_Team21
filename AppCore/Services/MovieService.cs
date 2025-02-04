using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AppCore.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Infrastructure.Interfaces;
using System;
using Infrastructure.Data;

namespace AppCore.Services
{
    public class MovieService : IMovieService
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MovieService> _logger;
        private readonly CinemaDbContext _context;

        public MovieService(
            IRepository<Movie> movieRepository, 
            IMapper mapper, 
            ILogger<MovieService> logger,
            CinemaDbContext context)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
            _logger = logger;
            _context = context;
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
            var movie = await _movieRepository.GetByIdWithIncludeAsync(id, 
                query => query
                    .Include(m => m.MovieGenres)
                        .ThenInclude(mg => mg.Genre)
                    .Include(m => m.MovieActors)
                        .ThenInclude(ma => ma.Actor));

            if (movie == null)
                return null;

            var movieDto = _mapper.Map<MovieDTO>(movie);
            
            if (movie.MovieGenres != null)
            {
                movieDto.SelectedGenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList();
            }
            
            if (movie.MovieActors != null)
            {
                movieDto.SelectedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList();
            }
            
            return movieDto;
        }

        public async Task AddMovieAsync(MovieDTO movieDto)
        {
            _logger.LogInformation("Adding new movie: {Title}", movieDto.Title);
            var movie = _mapper.Map<Movie>(movieDto);

            if (movieDto.SelectedGenreIds != null)
            {
                movie.MovieGenres = movieDto.SelectedGenreIds.Select(genreId => new MovieGenre
                {
                    GenreId = genreId
                }).ToList();
            }

            if (movieDto.SelectedActorIds != null)
            {
                movie.MovieActors = movieDto.SelectedActorIds.Select(actorId => new MovieActor
                {
                    ActorId = actorId
                }).ToList();
            }

            await _movieRepository.AddAsync(movie);
        }

        public async Task UpdateMovieAsync(MovieDTO movieDto)
        {
            _logger.LogInformation("Updating movie: {Id}", movieDto.MovieId);
            
            var existingMovie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.MovieId == movieDto.MovieId);

            if (existingMovie == null)
            {
                throw new Exception($"Movie not found with id: {movieDto.MovieId}");
            }

            _mapper.Map(movieDto, existingMovie);

            _context.MovieGenres.RemoveRange(existingMovie.MovieGenres);
            if (movieDto.SelectedGenreIds != null)
            {
                existingMovie.MovieGenres = movieDto.SelectedGenreIds.Select(genreId => new MovieGenre
                {
                    MovieId = existingMovie.MovieId,
                    GenreId = genreId
                }).ToList();
            }

            _context.MovieActors.RemoveRange(existingMovie.MovieActors);
            if (movieDto.SelectedActorIds != null)
            {
                existingMovie.MovieActors = movieDto.SelectedActorIds.Select(actorId => new MovieActor
                {
                    MovieId = existingMovie.MovieId,
                    ActorId = actorId
                }).ToList();
            }

            await _context.SaveChangesAsync();
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