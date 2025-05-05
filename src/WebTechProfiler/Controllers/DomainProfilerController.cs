using Microsoft.AspNetCore.Mvc;
using DnsClient;
using System.Net;

namespace WebTechProfiler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainProfilerController : ControllerBase
{
    private readonly ILogger<DomainProfilerController> _logger;
    private readonly ILookupClient _dnsClient;

    public DomainProfilerController(ILogger<DomainProfilerController> logger)
    {
        _logger = logger;
        _dnsClient = new LookupClient();
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeDomain([FromBody] DomainRequest request)
    {
        if (string.IsNullOrEmpty(request.Domain))
        {
            return BadRequest("Domain is required");
        }

        try
        {
            var domain = request.Domain.Trim().ToLower();
            var result = new DomainProfileResult
            {
                Domain = domain,
                Records = new Dictionary<string, List<string>>()
            };

            // Fetch different DNS record types
            result.Records["A"] = await GetRecords(domain, QueryType.A);
            result.Records["AAAA"] = await GetRecords(domain, QueryType.AAAA);
            result.Records["MX"] = await GetRecords(domain, QueryType.MX);
            result.Records["NS"] = await GetRecords(domain, QueryType.NS);
            result.Records["TXT"] = await GetRecords(domain, QueryType.TXT);
            result.Records["CNAME"] = await GetRecords(domain, QueryType.CNAME);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing domain {Domain}", request.Domain);
            return StatusCode(500, "Error analyzing domain: " + ex.Message);
        }
    }

    private async Task<List<string>> GetRecords(string domain, QueryType queryType)
    {
        try
        {
            var result = await _dnsClient.QueryAsync(domain, queryType);
            return result.Answers
                        .Select(r => r.ToString())
                        .Where(r => !string.IsNullOrEmpty(r))
                        .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public class DomainRequest
    {
        public string Domain { get; set; } = string.Empty;
    }

    public class DomainProfileResult
    {
        public string Domain { get; set; } = string.Empty;
        public Dictionary<string, List<string>> Records { get; set; } = new();
    }
} 