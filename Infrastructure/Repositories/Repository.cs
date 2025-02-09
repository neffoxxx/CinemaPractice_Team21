using Microsoft.EntityFrameworkCore;
using Infrastructure.Entities;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            var entityType = typeof(T);
            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id"));

            if (idProperty == null)
            {
                throw new InvalidOperationException($"No ID property found for entity {entityType.Name}");
            }

            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, idProperty);
            var constant = Expression.Constant(id);
            var equals = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            return await _dbSet.FirstOrDefaultAsync(lambda);
        }

        public virtual async Task<T?> GetByIdWithIncludeAsync(int id, Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeFunc)
        {
            var entityType = typeof(T);
            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id"));

            if (idProperty == null)
            {
                throw new InvalidOperationException($"No ID property found for entity {entityType.Name}");
            }

            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, idProperty);
            var constant = Expression.Constant(id);
            var equals = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            var query = _dbSet.AsQueryable();
            if (includeFunc != null)
            {
                query = includeFunc(query);
            }
            return await query.FirstOrDefaultAsync(lambda);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
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

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entityType = typeof(T);
            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id"));

            if (idProperty == null)
            {
                throw new InvalidOperationException($"No ID property found for entity {entityType.Name}");
            }

            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, idProperty);
            var constant = Expression.Constant(id);
            var equals = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            return await _dbSet.AnyAsync(lambda);
        }

        public virtual async Task<IEnumerable<T>> GetAllWithIncludeAsync(Func<IQueryable<T>, IQueryable<T>> include)
        {
            var query = _dbSet.AsQueryable();
            if (include != null)
            {
                query = include(query);
            }
            return await query.ToListAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}