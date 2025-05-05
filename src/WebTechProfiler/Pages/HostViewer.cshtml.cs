using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Text.RegularExpressions;

namespace WebTechProfiler.Pages;

public class HostViewerModel : PageModel
{
    private readonly ILogger<HostViewerModel> _logger;

    [BindProperty]
    public string Url { get; set; } = string.Empty;

    [BindProperty]
    public string IpAddress { get; set; } = string.Empty;

    public bool HasResult { get; private set; }
    public string OriginalDnsInfo { get; private set; } = string.Empty;
    public string ModifiedHostInfo { get; private set; } = string.Empty;

    public HostViewerModel(ILogger<HostViewerModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string ip, [FromQuery] string url)
    {
        if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(url))
        {
            IpAddress = ip;
            Url = url;
            await LoadHostInformation();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Strip protocol
        Url = Regex.Replace(Url, @"^(https?://)", "").TrimEnd('/');
        
        // Validate IP address format
        if (!IPAddress.TryParse(IpAddress, out _))
        {
            ModelState.AddModelError("IpAddress", "Please enter a valid IPv4 address");
            return Page();
        }

        await LoadHostInformation();
        return RedirectToPage("/HostViewer", new { ip = IpAddress, url = Url });
    }

    private async Task LoadHostInformation()
    {
        try
        {
            HasResult = true;
            var domain = new Uri("http://" + Url).Host;

            // Get original DNS information
            var originalIps = await Dns.GetHostAddressesAsync(domain);
            OriginalDnsInfo = $"Domain: {domain}\nIP Addresses:\n" + 
                string.Join("\n", originalIps.Select(ip => ip.ToString()));

            // Show modified host information
            ModifiedHostInfo = $"Domain: {domain}\nModified IP: {IpAddress}\n" +
                $"Equivalent to adding to hosts file:\n{IpAddress} {domain}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading host information for {Url}", Url);
            ModelState.AddModelError("", "Error retrieving host information. Please verify the URL is valid.");
            HasResult = false;
        }
    }
} 