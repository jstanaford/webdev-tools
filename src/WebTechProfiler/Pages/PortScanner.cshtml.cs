using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace WebTechProfiler.Pages;

public class PortScannerModel : PageModel
{
    private readonly ILogger<PortScannerModel> _logger;

    public PortScannerModel(ILogger<PortScannerModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
} 