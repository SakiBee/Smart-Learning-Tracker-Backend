namespace SLT.Core.Interfaces;

public class GeneratedFlashcard
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public interface IFlashcardGeneratorService
{
    Task<List<GeneratedFlashcard>> GenerateAsync(string title, string summary, string keyPoints);
}