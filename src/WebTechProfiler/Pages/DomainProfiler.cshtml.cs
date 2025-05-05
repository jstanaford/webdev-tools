using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTechProfiler.Pages;

public class DomainProfilerModel : PageModel
{
    private readonly ILogger<DomainProfilerModel> _logger;

    public DomainProfilerModel(ILogger<DomainProfilerModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
} 