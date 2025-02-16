using AppCore.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IHallService
    {
        Task<List<HallDTO>> GetAllHallsAsync();
        Task<HallDTO> GetHallByIdAsync(int id);
        Task AddHallAsync(HallDTO hallDto);
        Task UpdateHallAsync(HallDTO hallDto);
        Task DeleteHallAsync(int id);
    }
}
