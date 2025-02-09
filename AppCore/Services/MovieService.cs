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
using AppCore.Interfaces;

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

        public async Task<MovieDTO?> GetMovieByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting movie by ID: {Id}", id);

                var movie = await _movieRepository.GetByIdWithIncludeAsync(id,
                    query => query
                        .Include(m => m.MovieGenres)
                            .ThenInclude(mg => mg.Genre)
                        .Include(m => m.MovieActors)
                            .ThenInclude(ma => ma.Actor));

                if (movie == null)
                {
                    _logger.LogWarning("Movie not found with ID: {Id}", id);
                    return null;
                }

                var movieDto = _mapper.Map<MovieDTO>(movie);
                _logger.LogInformation("Successfully retrieved movie: {Title}", movieDto.Title);
                
                return movieDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie by ID: {Id}", id);
                throw;
            }
        }

        public async Task AddMovieAsync(MovieDTO movieDto)
        {
            _logger.LogInformation("Adding new movie: {Title}", movieDto.Title);
            var movie = _mapper.Map<Movie>(movieDto);

            if (movieDto.SelectedGenreIds != null)
            {
                var genres = await _context.Genres
                    .Where(g => movieDto.SelectedGenreIds.Contains(g.GenreId))
                    .ToListAsync();

                movie.MovieGenres = movieDto.SelectedGenreIds.Select(genreId => new MovieGenre
                {
                    GenreId = genreId,
                    Movie = movie,
                    Genre = genres.First(g => g.GenreId == genreId)
                }).ToList();
            }

            if (movieDto.SelectedActorIds != null)
            {
                var actors = await _context.Actors
                    .Where(a => movieDto.SelectedActorIds.Contains(a.ActorId))
                    .ToListAsync();

                movie.MovieActors = movieDto.SelectedActorIds.Select(actorId => new MovieActor
                {
                    ActorId = actorId,
                    Movie = movie,
                    Actor = actors.First(a => a.ActorId == actorId)
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
                var genres = await _context.Genres
                    .Where(g => movieDto.SelectedGenreIds.Contains(g.GenreId))
                    .ToListAsync();

                existingMovie.MovieGenres = movieDto.SelectedGenreIds.Select(genreId => new MovieGenre
                {
                    MovieId = existingMovie.MovieId,
                    GenreId = genreId,
                    Movie = existingMovie,
                    Genre = genres.First(g => g.GenreId == genreId)
                }).ToList();
            }

            _context.MovieActors.RemoveRange(existingMovie.MovieActors);
            if (movieDto.SelectedActorIds != null)
            {
                var actors = await _context.Actors
                    .Where(a => movieDto.SelectedActorIds.Contains(a.ActorId))
                    .ToListAsync();

                existingMovie.MovieActors = movieDto.SelectedActorIds.Select(actorId => new MovieActor
                {
                    MovieId = existingMovie.MovieId,
                    ActorId = actorId,
                    Movie = existingMovie,
                    Actor = actors.First(a => a.ActorId == actorId)
                }).ToList();
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteMovieAsync(int id)
        {
            _logger.LogInformation("Deleting movie by id: {Id}", id);
            await _movieRepository.DeleteAsync(id);
        }

        public List<MovieDTO> GetAllMovies()
        {
            var movies = _movieRepository.GetAllAsync().Result;
            return _mapper.Map<List<MovieDTO>>(movies);
        }
    }
}