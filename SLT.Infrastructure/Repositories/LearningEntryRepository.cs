using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;
using System.Linq.Expressions;

namespace SLT.Infrastructure.Repositories;

public class LearningEntryRepository : ILearningEntryRepository
{
    private readonly AppDbContext _context;

    public LearningEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LearningEntry?> GetByIdAsync(Guid id) =>
        await _context.LearningEntries.FindAsync(id);

    public async Task<IEnumerable<LearningEntry>> GetAllAsync() =>
        await _context.LearningEntries.ToListAsync();

    public async Task<IEnumerable<LearningEntry>> FindAsync(Expression<Func<LearningEntry, bool>> predicate) =>
        await _context.LearningEntries.Where(predicate).ToListAsync();

    public async Task<IEnumerable<LearningEntry>> GetByUserIdAsync(Guid userId) =>
        await _context.LearningEntries
            .Include(l => l.Tags)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<LearningEntry?> GetByIdWithTagsAsync(Guid id) =>
        await _context.LearningEntries
            .Include(l => l.Tags)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<bool> UrlExistsForUserAsync(string url, Guid userId) =>
        await _context.LearningEntries
            .AnyAsync(l => l.Url == url && l.UserId == userId);

    public async Task AddAsync(LearningEntry entity) =>
        await _context.LearningEntries.AddAsync(entity);

    public void Update(LearningEntry entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.LearningEntries.Update(entity);
    }

    public void Remove(LearningEntry entity) =>
        _context.LearningEntries.Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}