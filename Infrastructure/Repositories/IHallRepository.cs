using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public interface IHallRepository : IRepository<Hall>
    {
        new Task<IEnumerable<Hall>> GetAllAsync();
        new Task<Hall?> GetByIdAsync(int id);
        new Task AddAsync(Hall hall);
        new Task UpdateAsync(Hall hall);
        new Task DeleteAsync(int id);
        Task<IEnumerable<Hall>> GetActiveHallsAsync();
    }
} 