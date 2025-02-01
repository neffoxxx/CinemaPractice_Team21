using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class HallRepository : Repository<Hall>, IHallRepository
    {
        private readonly CinemaDbContext _context;

        public HallRepository(CinemaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Hall>> GetAllAsync()
        {
            return await _context.Halls.ToListAsync();
        }

        public async Task<IEnumerable<Hall>> GetAllWithIncludeAsync(Func<IQueryable<Hall>, IQueryable<Hall>> includeFunc)
        {
            var query = _context.Halls.AsQueryable();
            query = includeFunc(query);
            return await query.ToListAsync();
        }

        public async Task<Hall> GetByIdAsync(int id)
        {
            return await _context.Halls.FindAsync(id);
        }

        public async Task<Hall> GetByIdWithIncludeAsync(int id, Func<IQueryable<Hall>, IQueryable<Hall>> includeFunc)
        {
            var query = _context.Halls.AsQueryable();
            query = includeFunc(query);
            return await query.FirstOrDefaultAsync(h => h.HallId == id);
        }

        public async Task AddAsync(Hall hall)
        {
            await _context.Halls.AddAsync(hall);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Hall hall)
        {
            _context.Halls.Update(hall);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var hall = await GetByIdAsync(id);
            if (hall != null)
            {
                _context.Halls.Remove(hall);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Hall>> GetActiveHallsAsync()
        {
            return await _context.Halls
                .Where(h => h.IsActive)
                .ToListAsync();
        }

        public async Task<Hall> GetByIdWithSessionsAsync(int id)
        {
            return await _context.Halls
                .Include(h => h.Sessions)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(h => h.HallId == id);
        }
    }
} 