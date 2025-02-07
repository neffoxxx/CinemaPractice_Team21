using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Repositories
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        protected new readonly CinemaDbContext _context;

        public MovieRepository(CinemaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetAllWithDetailsAsync()
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Hall)
                .ToListAsync();
        }

        public async Task<Movie?> GetMovieWithDetailsAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.Sessions)
                    .ThenInclude(s => s.Hall)
                .FirstOrDefaultAsync(m => m.MovieId == id);
        }

        public async Task UpdateMovieDetailsAsync(Movie movie)
        {
            try
            {
                var existingMovie = await _context.Movies
                    .Include(m => m.MovieGenres)
                    .Include(m => m.MovieActors)
                    .FirstOrDefaultAsync(m => m.MovieId == movie.MovieId);

                if (existingMovie == null)
                {
                    throw new Exception($"Movie with ID {movie.MovieId} not found");
                }

                _context.Entry(existingMovie).CurrentValues.SetValues(movie);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating movie: {ex.Message}", ex);
            }
        }

        public override async Task<Movie?> GetByIdWithIncludeAsync(int id, Func<IQueryable<Movie>, IIncludableQueryable<Movie, object>>? includeFunc)
        {
            try
            {
                var query = _context.Movies.AsQueryable();
                if (includeFunc != null)
                {
                    query = includeFunc(query);
                }
                return await query.FirstOrDefaultAsync(m => m.MovieId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting movie with includes: {ex.Message}", ex);
            }
        }
    }
} 