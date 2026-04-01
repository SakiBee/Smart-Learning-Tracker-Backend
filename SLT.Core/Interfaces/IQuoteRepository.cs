using SLT.Core.Entities;

namespace SLT.Core.Interfaces;

public interface IQuoteRepository : IRepository<Quote>
{
    Task<IEnumerable<Quote>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Quote>> GetByEntryIdAsync(Guid entryId, Guid userId);
}