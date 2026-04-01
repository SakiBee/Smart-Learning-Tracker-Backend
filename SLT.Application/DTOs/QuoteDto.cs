namespace SLT.Application.DTOs;

public class QuoteDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Color { get; set; }
    public Guid LearningEntryId { get; set; }
    public string EntryTitle { get; set; } = string.Empty;
    public string EntryUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateQuoteDto
{
    public string Text { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Color { get; set; } = "yellow";
    public Guid LearningEntryId { get; set; }
}

public class UpdateQuoteDto
{
    public string? Note { get; set; }
    public string? Color { get; set; }
}