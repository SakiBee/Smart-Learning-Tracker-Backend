using SLT.Core.Entities;

namespace SLT.Core.Specifications;

// All quotes for a user
public class QuotesByUserSpec : BaseSpecification<Quote>
{
    public QuotesByUserSpec(Guid userId)
        : base(q => q.UserId == userId)
    {
        AddInclude(q => q.LearningEntry);
        ApplyOrderByDescending(q => q.CreatedAt);
    }
}

// Quotes by entry ID for a user
public class QuotesByEntrySpec : BaseSpecification<Quote>
{
    public QuotesByEntrySpec(Guid entryId, Guid userId)
        : base(q => q.LearningEntryId == entryId && q.UserId == userId)
    {
        ApplyOrderByDescending(q => q.CreatedAt);
    }
}

// Single quote owned by user
public class QuoteByIdAndUserSpec : BaseSpecification<Quote>
{
    public QuoteByIdAndUserSpec(Guid id, Guid userId)
        : base(q => q.Id == id && q.UserId == userId) { }
}