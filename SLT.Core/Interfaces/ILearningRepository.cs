using SLT.Core.Entities;

namespace SLT.Core.Interfaces;

public interface ILearningEntryRepository : IRepository<LearningEntry>
{
    Task<IEnumerable<LearningEntry>> GetByUserIdAsync(Guid userId);
    Task<LearningEntry?> GetByIdWithTagsAsync(Guid id);
    Task<bool> UrlExistsForUserAsync(string url, Guid userId);
}