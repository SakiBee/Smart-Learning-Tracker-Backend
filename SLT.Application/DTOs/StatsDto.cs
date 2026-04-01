namespace SLT.Application.DTOs;

public class StatsDto
{
    public int TotalSaved { get; set; }
    public int TotalRead { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalReadLater { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int SavedThisWeek { get; set; }
    public int ReadThisWeek { get; set; }
    public List<DailyActivityDto> Last30DaysActivity { get; set; } = new();
    public List<TopTagDto> TopTags { get; set; } = new();
    public List<ContentTypeStatDto> ContentTypeBreakdown { get; set; } = new();
    public List<WeeklyDigestEntryDto> WeeklyDigest { get; set; } = new();
}

public class DailyActivityDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopTagDto
{
    public string Tag { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ContentTypeStatDto
{
    public string ContentType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WeeklyDigestEntryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}