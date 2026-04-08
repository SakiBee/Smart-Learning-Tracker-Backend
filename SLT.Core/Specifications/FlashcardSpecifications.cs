using SLT.Core.Entities;

namespace SLT.Core.Specifications;

// All flashcards for a user
public class FlashcardsByUserSpec : BaseSpecification<Flashcard>
{
    public FlashcardsByUserSpec(Guid userId)
        : base(f => f.UserId == userId)
    {
        AddInclude(f => f.LearningEntry);
        ApplyOrderBy(f => f.NextReviewAt);
    }
}

// Flashcards due for review
public class DueFlashcardsSpec : BaseSpecification<Flashcard>
{
    public DueFlashcardsSpec(Guid userId)
        : base(f => f.UserId == userId && f.NextReviewAt <= DateTime.UtcNow)
    {
        AddInclude(f => f.LearningEntry);
        ApplyOrderBy(f => f.NextReviewAt);
    }
}

// Flashcards by entry ID
public class FlashcardsByEntrySpec : BaseSpecification<Flashcard>
{
    public FlashcardsByEntrySpec(Guid entryId, Guid userId)
        : base(f => f.LearningEntryId == entryId && f.UserId == userId) { }
}

// Single flashcard with entry
public class FlashcardWithEntrySpec : BaseSpecification<Flashcard>
{
    public FlashcardWithEntrySpec(Guid id)
        : base(f => f.Id == id)
    {
        AddInclude(f => f.LearningEntry);
    }
}

// Flashcard owned by user
public class FlashcardByIdAndUserSpec : BaseSpecification<Flashcard>
{
    public FlashcardByIdAndUserSpec(Guid id, Guid userId)
        : base(f => f.Id == id && f.UserId == userId)
    {
        AddInclude(f => f.LearningEntry);
    }
}