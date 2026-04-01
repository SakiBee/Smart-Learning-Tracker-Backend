namespace SLT.Application.DTOs;

public class FlashcardDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int TimesReviewed { get; set; }
    public int TimesCorrect { get; set; }
    public DateTime NextReviewAt { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public int Interval { get; set; }
    public Guid LearningEntryId { get; set; }
    public string EntryTitle { get; set; } = string.Empty;
    public bool IsDue { get; set; }
    public double AccuracyRate { get; set; }
}

public class GenerateFlashcardsDto
{
    public Guid LearningEntryId { get; set; }
}

public class ReviewFlashcardDto
{
    // 0 = Again, 1 = Hard, 2 = Good, 3 = Easy
    public int Rating { get; set; }
}

public class CreateFlashcardDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public Guid LearningEntryId { get; set; }
}