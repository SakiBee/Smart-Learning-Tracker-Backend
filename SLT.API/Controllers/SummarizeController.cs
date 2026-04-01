using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Interfaces;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SummarizeController : ControllerBase
{
    private readonly IContentExtractorService _extractor;
    private readonly IAiSummaryService _aiSummary;

    public SummarizeController(
        IContentExtractorService extractor,
        IAiSummaryService aiSummary)
    {
        _extractor = extractor;
        _aiSummary = aiSummary;
    }

    [HttpPost]
    public async Task<IActionResult> Summarize([FromBody] UrlSummaryRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Url))
            return BadRequest(new { message = "URL is required." });

        if (!Uri.TryCreate(dto.Url, UriKind.Absolute, out _))
            return BadRequest(new { message = "Invalid URL format." });

        // Step 1 — Extract page content
        var extracted = await _extractor.ExtractAsync(dto.Url);

        // Step 2 — Generate AI summary
        var contentForAi = !string.IsNullOrWhiteSpace(extracted.RawText)
            ? extracted.RawText
            : extracted.Description ?? extracted.Title;

        var aiResult = await _aiSummary.SummarizeAsync(
            extracted.Title,
            contentForAi,
            dto.Url);

        // Step 3 — Build response
        var response = new UrlSummaryResponseDto
        {
            Url = dto.Url,
            Title = extracted.Title,
            Author = extracted.Author,
            ThumbnailUrl = extracted.ThumbnailUrl,
            Description = extracted.Description,
            Summary = aiResult.Summary,
            KeyPoints = aiResult.KeyPoints,
            SuggestedTags = aiResult.SuggestedTags,
            ContentType = aiResult.ContentType,
            Domain = extracted.Domain
        };

        return Ok(response);
    }
}