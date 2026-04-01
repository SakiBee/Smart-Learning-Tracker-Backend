using SLT.Core.Entities;

namespace SLT.Core.Interfaces;

public interface ICollectionRepository : IRepository<Collection>
{
    Task<IEnumerable<Collection>> GetByUserIdAsync(Guid userId);
    Task<Collection?> GetByIdWithEntriesAsync(Guid id);
    Task<Collection?> GetByShareSlugAsync(string slug);
    Task<bool> EntryExistsInCollectionAsync(Guid collectionId, Guid entryId);
}