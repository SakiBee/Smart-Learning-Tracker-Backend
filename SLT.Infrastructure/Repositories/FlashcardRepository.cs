using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;
using System.Linq.Expressions;

namespace SLT.Infrastructure.Repositories;

public class FlashcardRepository : IFlashcardRepository
{
    private readonly AppDbContext _context;

    public FlashcardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Flashcard?> GetByIdAsync(Guid id) =>
        await _context.Flashcards.Include(f => f.LearningEntry).FirstOrDefaultAsync(f => f.Id == id);

    public async Task<IEnumerable<Flashcard>> GetAllAsync() =>
        await _context.Flashcards.ToListAsync();

    public async Task<IEnumerable<Flashcard>> FindAsync(Expression<Func<Flashcard, bool>> predicate) =>
        await _context.Flashcards.Where(predicate).ToListAsync();

    public async Task<IEnumerable<Flashcard>> GetByUserIdAsync(Guid userId) =>
        await _context.Flashcards
            .Include(f => f.LearningEntry)
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.NextReviewAt)
            .ToListAsync();

    public async Task<IEnumerable<Flashcard>> GetDueForReviewAsync(Guid userId) =>
        await _context.Flashcards
            .Include(f => f.LearningEntry)
            .Where(f => f.UserId == userId && f.NextReviewAt <= DateTime.UtcNow)
            .OrderBy(f => f.NextReviewAt)
            .ToListAsync();

    public async Task<IEnumerable<Flashcard>> GetByEntryIdAsync(Guid entryId, Guid userId) =>
        await _context.Flashcards
            .Where(f => f.LearningEntryId == entryId && f.UserId == userId)
            .ToListAsync();

    public async Task<int> GetTotalCountAsync(Guid userId) =>
        await _context.Flashcards.CountAsync(f => f.UserId == userId);

    public async Task AddAsync(Flashcard entity) =>
        await _context.Flashcards.AddAsync(entity);

    public void Update(Flashcard entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Flashcards.Update(entity);
    }

    public void Remove(Flashcard entity) =>
        _context.Flashcards.Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}