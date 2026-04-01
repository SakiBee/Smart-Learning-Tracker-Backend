using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Interfaces;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly ILearningEntryRepository _repository;

    public StatsController(ILearningEntryRepository repository)
    {
        _repository = repository;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var entries = (await _repository.GetByUserIdAsync(CurrentUserId)).ToList();
        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);

        // ── Basic counts ──────────────────────────────────────────────────────
        var stats = new StatsDto
        {
            TotalSaved = entries.Count,
            TotalRead = entries.Count(e => e.IsRead),
            TotalFavorites = entries.Count(e => e.IsFavorite),
            TotalReadLater = entries.Count(e => e.IsReadLater),
            SavedThisWeek = entries.Count(e => e.CreatedAt >= startOfWeek),
            ReadThisWeek = entries.Count(e => e.IsRead && e.UpdatedAt >= startOfWeek),
        };

        // ── Streak calculation ────────────────────────────────────────────────
        var activeDates = entries
            .Select(e => e.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int currentStreak = 0;
        int longestStreak = 0;
        int tempStreak = 0;
        var checkDate = now.Date;

        // Current streak
        foreach (var date in activeDates)
        {
            if (date == checkDate || date == checkDate.AddDays(-1))
            {
                currentStreak++;
                checkDate = date;
            }
            else break;
        }

        // Longest streak
        var sortedDates = activeDates.OrderBy(d => d).ToList();
        for (int i = 0; i < sortedDates.Count; i++)
        {
            if (i == 0 || sortedDates[i] == sortedDates[i - 1].AddDays(1))
            {
                tempStreak++;
                longestStreak = Math.Max(longestStreak, tempStreak);
            }
            else
            {
                tempStreak = 1;
            }
        }

        stats.CurrentStreak = currentStreak;
        stats.LongestStreak = longestStreak;

        // ── Last 30 days activity ─────────────────────────────────────────────
        stats.Last30DaysActivity = Enumerable.Range(0, 30)
            .Select(i => now.Date.AddDays(-29 + i))
            .Select(date => new DailyActivityDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = entries.Count(e => e.CreatedAt.Date == date)
            })
            .ToList();

        // ── Top tags ──────────────────────────────────────────────────────────
        stats.TopTags = entries
            .SelectMany(e => e.Tags.Select(t => t.Name))
            .GroupBy(t => t)
            .Select(g => new TopTagDto { Tag = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .Take(8)
            .ToList();

        // ── Content type breakdown ────────────────────────────────────────────
        stats.ContentTypeBreakdown = entries
            .GroupBy(e => e.ContentType.ToString())
            .Select(g => new ContentTypeStatDto
            {
                ContentType = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        // ── Weekly digest (last 7 days) ───────────────────────────────────────
        stats.WeeklyDigest = entries
            .Where(e => e.CreatedAt >= now.AddDays(-7))
            .OrderByDescending(e => e.CreatedAt)
            .Take(10)
            .Select(e => new WeeklyDigestEntryDto
            {
                Id = e.Id,
                Title = e.Title,
                Url = e.Url,
                Summary = e.Summary,
                Tags = e.Tags.Select(t => t.Name).ToList(),
                IsRead = e.IsRead,
                CreatedAt = e.CreatedAt
            })
            .ToList();

        return Ok(stats);
    }
}