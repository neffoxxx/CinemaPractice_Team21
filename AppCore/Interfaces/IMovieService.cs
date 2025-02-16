using AppCore.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieDTO>> GetAllMoviesAsync();
        Task<MovieDTO?> GetMovieByIdAsync(int id);
        Task AddMovieAsync(MovieDTO movieDto);
        Task UpdateMovieAsync(MovieDTO movieDto);
        Task DeleteMovieAsync(int id);
    }
}
