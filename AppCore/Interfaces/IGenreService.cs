using AppCore.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreDTO>> GetAllGenresAsync();
        Task<GenreDTO> GetGenreByIdAsync(int id);
        Task AddGenreAsync(GenreDTO genreDto);
        Task UpdateGenreAsync(GenreDTO genreDto);
        Task DeleteGenreAsync(int id);
    }
}