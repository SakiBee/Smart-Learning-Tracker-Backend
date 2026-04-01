using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;
using System.Linq.Expressions;

namespace SLT.Infrastructure.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly AppDbContext _context;

    public QuoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Quote?> GetByIdAsync(Guid id) =>
        await _context.Quotes.FindAsync(id);

    public async Task<IEnumerable<Quote>> GetAllAsync() =>
        await _context.Quotes.ToListAsync();

    public async Task<IEnumerable<Quote>> FindAsync(Expression<Func<Quote, bool>> predicate) =>
        await _context.Quotes.Where(predicate).ToListAsync();

    public async Task<IEnumerable<Quote>> GetByUserIdAsync(Guid userId) =>
        await _context.Quotes
            .Include(q => q.LearningEntry)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Quote>> GetByEntryIdAsync(Guid entryId, Guid userId) =>
        await _context.Quotes
            .Where(q => q.LearningEntryId == entryId && q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Quote entity) =>
        await _context.Quotes.AddAsync(entity);

    public void Update(Quote entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Quotes.Update(entity);
    }

    public void Remove(Quote entity) =>
        _context.Quotes.Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}