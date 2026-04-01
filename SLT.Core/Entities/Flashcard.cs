namespace SLT.Core.Entities;

public class Flashcard : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int TimesReviewed { get; set; } = 0;
    public int TimesCorrect { get; set; } = 0;
    public DateTime? LastReviewedAt { get; set; }
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public int EaseFactor { get; set; } = 250; // SM-2 algorithm (x100)
    public int Interval { get; set; } = 1;     // days

    public Guid LearningEntryId { get; set; }
    public LearningEntry LearningEntry { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}