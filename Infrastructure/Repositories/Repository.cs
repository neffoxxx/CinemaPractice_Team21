using Microsoft.EntityFrameworkCore;
using Infrastructure.Entities;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CinemaDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(CinemaDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<T> GetByIdWithIncludeAsync(int id, Func<IQueryable<T>, IQueryable<T>> include)
        {
            IQueryable<T> query = _dbSet;
            
            if (include != null)
            {
                query = include(query);
            }

            // Отримуємо ім'я ID властивості для конкретного типу
            var idPropertyName = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id"))?.Name ?? "Id";

            // Створюємо динамічний вираз для пошуку по ID
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, idPropertyName);
            var value = Expression.Constant(id);
            var equals = Expression.Equal(property, value);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            return await query.FirstOrDefaultAsync(lambda);
        }

        public async Task<IEnumerable<T>> GetAllWithIncludeAsync(Func<IQueryable<T>, IQueryable<T>> include)
        {
            IQueryable<T> query = _dbSet;
            
            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }
    }
}
