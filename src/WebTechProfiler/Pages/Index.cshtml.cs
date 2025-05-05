using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTechProfiler.Models;
using WebTechProfiler.Services;

namespace WebTechProfiler.Pages;

public class IndexModel : PageModel
{
    private readonly IWebTechnologyAnalyzer _analyzer;
    private readonly ILogger<IndexModel> _logger;

    [BindProperty]
    public string TargetUrl { get; set; } = string.Empty;

    public TechnologyProfile? Profile { get; private set; }

    public IndexModel(IWebTechnologyAnalyzer analyzer, ILogger<IndexModel> logger)
    {
        _analyzer = analyzer;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!Uri.TryCreate(TargetUrl, UriKind.Absolute, out _))
        {
            ModelState.AddModelError("TargetUrl", "Please enter a valid URL (e.g. https://www.example.com)");
            return Page();
        }

        try
        {
            Profile = await _analyzer.AnalyzeWebsiteAsync(TargetUrl);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing website");
            ModelState.AddModelError("", "An error occurred while analyzing the website. Please try again.");
            return Page();
        }
    }
}
