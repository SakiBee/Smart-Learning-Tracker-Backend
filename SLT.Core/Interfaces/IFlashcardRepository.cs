using SLT.Core.Entities;

namespace SLT.Core.Interfaces;

public interface IFlashcardRepository : IRepository<Flashcard>
{
    Task<IEnumerable<Flashcard>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Flashcard>> GetDueForReviewAsync(Guid userId);
    Task<IEnumerable<Flashcard>> GetByEntryIdAsync(Guid entryId, Guid userId);
    Task<int> GetTotalCountAsync(Guid userId);
}