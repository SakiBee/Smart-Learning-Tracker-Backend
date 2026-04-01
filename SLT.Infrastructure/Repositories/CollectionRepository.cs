using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;
using System.Linq.Expressions;

namespace SLT.Infrastructure.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly AppDbContext _context;

    public CollectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Collection?> GetByIdAsync(Guid id) =>
        await _context.Collections.FindAsync(id);

    public async Task<IEnumerable<Collection>> GetAllAsync() =>
        await _context.Collections.ToListAsync();

    public async Task<IEnumerable<Collection>> FindAsync(Expression<Func<Collection, bool>> predicate) =>
        await _context.Collections.Where(predicate).ToListAsync();

    public async Task<IEnumerable<Collection>> GetByUserIdAsync(Guid userId) =>
        await _context.Collections
            .Include(c => c.CollectionEntries)
                .ThenInclude(ce => ce.LearningEntry)
                    .ThenInclude(le => le.Tags)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Collection?> GetByIdWithEntriesAsync(Guid id) =>
        await _context.Collections
            .Include(c => c.CollectionEntries)
                .ThenInclude(ce => ce.LearningEntry)
                    .ThenInclude(le => le.Tags)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Collection?> GetByShareSlugAsync(string slug) =>
        await _context.Collections
            .Include(c => c.CollectionEntries)
                .ThenInclude(ce => ce.LearningEntry)
                    .ThenInclude(le => le.Tags)
            .FirstOrDefaultAsync(c => c.ShareSlug == slug && c.IsPublic);

    public async Task<bool> EntryExistsInCollectionAsync(Guid collectionId, Guid entryId) =>
        await _context.CollectionEntries
            .AnyAsync(ce => ce.CollectionId == collectionId && ce.LearningEntryId == entryId);

    public async Task AddAsync(Collection entity) =>
        await _context.Collections.AddAsync(entity);

    public void Update(Collection entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Collections.Update(entity);
    }

    public void Remove(Collection entity) =>
        _context.Collections.Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}