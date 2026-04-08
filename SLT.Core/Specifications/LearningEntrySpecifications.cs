using SLT.Core.Entities;

namespace SLT.Core.Specifications;

// All entries for a user ordered by newest first
public class EntriesByUserSpec : BaseSpecification<LearningEntry>
{
    public EntriesByUserSpec(Guid userId)
        : base(e => e.UserId == userId)
    {
        AddInclude(e => e.Tags);
        ApplyOrderByDescending(e => e.CreatedAt);
    }
}

// Single entry with tags by ID
public class EntryWithTagsByIdSpec : BaseSpecification<LearningEntry>
{
    public EntryWithTagsByIdSpec(Guid id)
        : base(e => e.Id == id)
    {
        AddInclude(e => e.Tags);
    }
}

// Single entry owned by user (security check)
public class EntryByIdAndUserSpec : BaseSpecification<LearningEntry>
{
    public EntryByIdAndUserSpec(Guid id, Guid userId)
        : base(e => e.Id == id && e.UserId == userId)
    {
        AddInclude(e => e.Tags);
    }
}

// Check for duplicate URL per user
public class EntryUrlExistsSpec : BaseSpecification<LearningEntry>
{
    public EntryUrlExistsSpec(string url, Guid userId)
        : base(e => e.Url == url && e.UserId == userId) { }
}

// Favorites only
public class FavoriteEntriesByUserSpec : BaseSpecification<LearningEntry>
{
    public FavoriteEntriesByUserSpec(Guid userId)
        : base(e => e.UserId == userId && e.IsFavorite)
    {
        AddInclude(e => e.Tags);
        ApplyOrderByDescending(e => e.CreatedAt);
    }
}

// Read Later entries
public class ReadLaterEntriesByUserSpec : BaseSpecification<LearningEntry>
{
    public ReadLaterEntriesByUserSpec(Guid userId)
        : base(e => e.UserId == userId && e.IsReadLater)
    {
        AddInclude(e => e.Tags);
        ApplyOrderByDescending(e => e.CreatedAt);
    }
}

// Unread entries
public class UnreadEntriesByUserSpec : BaseSpecification<LearningEntry>
{
    public UnreadEntriesByUserSpec(Guid userId)
        : base(e => e.UserId == userId && !e.IsRead)
    {
        AddInclude(e => e.Tags);
        ApplyOrderByDescending(e => e.CreatedAt);
    }
}

// Entries saved within last N days
public class RecentEntriesByUserSpec : BaseSpecification<LearningEntry>
{
    public RecentEntriesByUserSpec(Guid userId, int days)
        : base(e => e.UserId == userId && e.CreatedAt >= DateTime.UtcNow.AddDays(-days))
    {
        AddInclude(e => e.Tags);
        ApplyOrderByDescending(e => e.CreatedAt);
    }
}