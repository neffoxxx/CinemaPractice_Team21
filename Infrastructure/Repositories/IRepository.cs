public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllWithIncludeAsync(Func<IQueryable<T>, IQueryable<T>> include);
    Task<T> GetByIdAsync(int id);
    Task<T> GetByIdWithIncludeAsync(int id, Func<IQueryable<T>, IQueryable<T>> include);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}