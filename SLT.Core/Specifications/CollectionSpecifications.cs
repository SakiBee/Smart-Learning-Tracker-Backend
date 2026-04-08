using SLT.Core.Entities;

namespace SLT.Core.Specifications;

// All collections for a user
public class CollectionsByUserSpec : BaseSpecification<Collection>
{
    public CollectionsByUserSpec(Guid userId)
        : base(c => c.UserId == userId)
    {
        AddInclude("CollectionEntries.LearningEntry.Tags");
        ApplyOrderByDescending(c => c.CreatedAt);
    }
}

// Single collection with all entries
public class CollectionWithEntriesSpec : BaseSpecification<Collection>
{
    public CollectionWithEntriesSpec(Guid id)
        : base(c => c.Id == id)
    {
        AddInclude("CollectionEntries.LearningEntry.Tags");
    }
}

// Single collection owned by user
public class CollectionByIdAndUserSpec : BaseSpecification<Collection>
{
    public CollectionByIdAndUserSpec(Guid id, Guid userId)
        : base(c => c.Id == id && c.UserId == userId) { }
}

// Public collection by share slug
public class PublicCollectionBySlugSpec : BaseSpecification<Collection>
{
    public PublicCollectionBySlugSpec(string slug)
        : base(c => c.ShareSlug == slug && c.IsPublic)
    {
        AddInclude("CollectionEntries.LearningEntry.Tags");
    }
}

// Check if entry exists in collection
public class CollectionEntryExistsSpec : BaseSpecification<CollectionEntry>
{
    public CollectionEntryExistsSpec(Guid collectionId, Guid entryId)
        : base(ce => ce.CollectionId == collectionId && ce.LearningEntryId == entryId) { }
}

// Collection entry by collection + entry IDs
public class CollectionEntryByIdsSpec : BaseSpecification<CollectionEntry>
{
    public CollectionEntryByIdsSpec(Guid collectionId, Guid entryId)
        : base(ce => ce.CollectionId == collectionId && ce.LearningEntryId == entryId) { }
}