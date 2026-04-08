using SLT.Core.Specifications;

namespace SLT.Core.Interfaces;

public interface IRepository<T> where T : class
{
    // Basic CRUD
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();

    // Specification methods
    Task<T?> GetEntityWithSpec(ISpecification<T> spec);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
    Task<int> CountAsync(ISpecification<T> spec);
    Task<bool> AnyAsync(ISpecification<T> spec);
}