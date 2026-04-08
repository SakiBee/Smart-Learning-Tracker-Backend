using Microsoft.EntityFrameworkCore;
using SLT.Core.Interfaces;
using SLT.Core.Specifications;
using SLT.Infrastructure.Data;

namespace SLT.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id) =>
        await _context.Set<T>().FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _context.Set<T>().ToListAsync();

    public async Task<T?> GetEntityWithSpec(ISpecification<T> spec) =>
        await ApplySpecification(spec).FirstOrDefaultAsync();

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec) =>
        await ApplySpecification(spec).ToListAsync();

    public async Task<int> CountAsync(ISpecification<T> spec) =>
        await ApplySpecification(spec).CountAsync();

    public async Task<bool> AnyAsync(ISpecification<T> spec) =>
        await ApplySpecification(spec).AnyAsync();

    public async Task AddAsync(T entity) =>
        await _context.Set<T>().AddAsync(entity);

    public void Update(T entity) =>
        _context.Set<T>().Update(entity);

    public void Remove(T entity) =>
        _context.Set<T>().Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    private IQueryable<T> ApplySpecification(ISpecification<T> spec) =>
        SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
}