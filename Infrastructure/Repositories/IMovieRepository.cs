using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<IEnumerable<Movie>> GetAllWithDetailsAsync();
        Task<Movie?> GetMovieWithDetailsAsync(int id);
        Task UpdateMovieDetailsAsync(Movie movie);
    }
}
