using Microsoft.AspNetCore.Mvc;
using WebTechProfiler.Models;
using WebTechProfiler.Services;

namespace WebTechProfiler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzerController : ControllerBase
{
    private readonly IWebTechnologyAnalyzer _analyzer;
    private readonly ILogger<AnalyzerController> _logger;

    public AnalyzerController(IWebTechnologyAnalyzer analyzer, ILogger<AnalyzerController> logger)
    {
        _analyzer = analyzer;
        _logger = logger;
    }

    [HttpGet("analyze")]
    public async Task<ActionResult<TechnologyProfile>> AnalyzeWebsite([FromQuery] string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return BadRequest("Invalid URL format");
        }

        try
        {
            var profile = await _analyzer.AnalyzeWebsiteAsync(url);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing website");
            return StatusCode(500, "An error occurred while analyzing the website");
        }
    }
} 