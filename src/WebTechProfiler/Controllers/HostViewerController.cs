using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebTechProfiler.Controllers;

[ApiController]
[Route("api/hostviewer")]
public class HostViewerController : ControllerBase
{
    private readonly ILogger<HostViewerController> _logger;

    public HostViewerController(ILogger<HostViewerController> logger)
    {
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] HostAnalysisRequest request)
    {
        try
        {
            var originalIps = await Dns.GetHostAddressesAsync(request.Domain);
            
            return Ok(new
            {
                OriginalDnsInfo = $"Domain: {request.Domain}\nIP Addresses:\n" + 
                    string.Join("\n", originalIps.Select(ip => ip.ToString())),
                ModifiedHostInfo = $"Domain: {request.Domain}\nModified IP: {request.IpAddress}\n" +
                    $"Equivalent to adding to hosts file:\n{request.IpAddress} {request.Domain}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing host for {Domain}", request.Domain);
            return BadRequest("Error analyzing host information");
        }
    }
}

public class HostAnalysisRequest
{
    public string Domain { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
} 