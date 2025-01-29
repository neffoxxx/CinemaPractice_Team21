using Infrastructure.Entities;

namespace Infrastructure.Repositories
{
    public interface IHallRepository : IRepository<Hall>
    {
        Task<IEnumerable<Hall>> GetAllAsync();
        Task<Hall> GetByIdAsync(int id);
        Task AddAsync(Hall hall);
        Task UpdateAsync(Hall hall);
        Task DeleteAsync(int id);
        Task<IEnumerable<Hall>> GetActiveHallsAsync();
    }
} 