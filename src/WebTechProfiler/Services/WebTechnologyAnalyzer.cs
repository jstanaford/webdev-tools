using System.Text.RegularExpressions;
using WebTechProfiler.Models;

namespace WebTechProfiler.Services;

public interface IWebTechnologyAnalyzer
{
    Task<TechnologyProfile> AnalyzeWebsiteAsync(string url);
}

public class WebTechnologyAnalyzer : IWebTechnologyAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebTechnologyAnalyzer> _logger;

    public WebTechnologyAnalyzer(HttpClient httpClient, ILogger<WebTechnologyAnalyzer> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TechnologyProfile> AnalyzeWebsiteAsync(string url)
    {
        var profile = new TechnologyProfile
        {
            Url = url,
            ScannedAt = DateTime.UtcNow
        };

        try
        {
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Analyze headers
            foreach (var header in response.Headers)
            {
                profile.Headers[header.Key] = string.Join(", ", header.Value);
            }

            // Detect server type
            if (response.Headers.Server.Any())
            {
                profile.ServerType = response.Headers.Server.First().ToString();
            }

            // Basic JavaScript library detection
            var jsLibraries = DetectJavaScriptLibraries(content);
            profile.JavaScriptLibraries.AddRange(jsLibraries);

            // Meta technology detection
            var metaTechnologies = DetectMetaTechnologies(content);
            profile.MetaTechnologies.AddRange(metaTechnologies);

            // Technology fingerprinting
            var detectedTechnologies = DetectTechnologies(content, profile.Headers);
            profile.DetectedTechnologies.AddRange(detectedTechnologies);

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing website: {Url}", url);
            throw;
        }
    }

    private Dictionary<string, (string Pattern, string[] CDNs)> GetLibraryPatterns()
    {
        return new Dictionary<string, (string Pattern, string[] CDNs)>
        {
            { 
                "jQuery", 
                (@"jquery[.-](\d+\.\d+\.\d+)", 
                new[] { 
                    "code.jquery.com/jquery", 
                    "ajax.googleapis.com/ajax/libs/jquery",
                    "cdnjs.cloudflare.com/ajax/libs/jquery"
                })
            },
            { 
                "React", 
                (@"react(?:\.production)?(?:\.min)?\.js.*?(\d+\.\d+\.\d+)", 
                new[] { 
                    "unpkg.com/react",
                    "cdn.jsdelivr.net/npm/react",
                    "cdnjs.cloudflare.com/ajax/libs/react"
                })
            },
            {
                "Next.js",
                (@"next(?:/dist)?/(?:static|client)/.*?(\d+\.\d+\.\d+)",
                new[] { "/_next/static/" })
            },
            {
                "Vue.js",
                (@"vue(?:\.runtime)?(?:\.min)?\.js.*?(\d+\.\d+\.\d+)",
                new[] { 
                    "unpkg.com/vue",
                    "cdn.jsdelivr.net/npm/vue",
                    "cdnjs.cloudflare.com/ajax/libs/vue"
                })
            },
            {
                "Angular",
                (@"angular(?:\.min)?\.js.*?(\d+\.\d+\.\d+)",
                new[] { 
                    "ajax.googleapis.com/ajax/libs/angularjs",
                    "unpkg.com/@angular/core",
                    "cdn.jsdelivr.net/npm/@angular/core"
                })
            },
            {
                "Tailwind CSS",
                (@"tailwindcss@(\d+\.\d+\.\d+)",
                new[] { 
                    "cdn.tailwindcss.com",
                    "unpkg.com/tailwindcss",
                    "cdn.jsdelivr.net/npm/tailwindcss"
                })
            },
            {
                "Material-UI",
                (@"@mui/material.*?(\d+\.\d+\.\d+)",
                new[] { 
                    "unpkg.com/@mui/material",
                    "cdn.jsdelivr.net/npm/@mui/material"
                })
            },
            {
                "Lodash",
                (@"lodash(?:\.min)?\.js.*?(\d+\.\d+\.\d+)",
                new[] { 
                    "cdn.jsdelivr.net/npm/lodash",
                    "cdnjs.cloudflare.com/ajax/libs/lodash"
                })
            },
            {
                "Moment.js",
                (@"moment(?:\.min)?\.js.*?(\d+\.\d+\.\d+)",
                new[] { 
                    "momentjs.com/downloads/moment.js",
                    "cdn.jsdelivr.net/npm/moment",
                    "cdnjs.cloudflare.com/ajax/libs/moment.js"
                })
            },
            {
                "Redux",
                (@"redux(?:\.min)?\.js.*?(\d+\.\d+\.\d+)",
                new[] { 
                    "unpkg.com/redux",
                    "cdn.jsdelivr.net/npm/redux"
                })
            }
        };
    }

    private List<JavaScriptLibraryInfo> DetectJavaScriptLibraries(string content)
    {
        var libraries = new List<JavaScriptLibraryInfo>();
        var patterns = GetLibraryPatterns();
        
        // Debug log the content length
        _logger.LogInformation("Content length: {Length} characters", content.Length);
        
        // Look for script tags
        var scriptPattern = @"<script[^>]*src=[""']([^""']+)[""'][^>]*>";
        var scriptMatches = Regex.Matches(content, scriptPattern, RegexOptions.IgnoreCase);
        
        // Debug log found script tags
        _logger.LogInformation("Found {Count} script tags", scriptMatches.Count);
        
        foreach (Match scriptMatch in scriptMatches)
        {
            var src = scriptMatch.Groups[1].Value;
            _logger.LogInformation("Analyzing script source: {Src}", src);
            
            foreach (var lib in patterns)
            {
                // Check CDN paths
                var cdnMatch = lib.Value.CDNs.Any(cdn => 
                    src.Contains(cdn, StringComparison.OrdinalIgnoreCase));
                
                if (cdnMatch)
                {
                    _logger.LogInformation("Found CDN match for {Library}", lib.Key);
                    var versionMatch = Regex.Match(src, lib.Value.Pattern, RegexOptions.IgnoreCase);
                    _logger.LogInformation("Version match success: {Success}, Version: {Version}", 
                        versionMatch.Success, 
                        versionMatch.Success ? versionMatch.Groups[1].Value : "none");
                    
                    libraries.Add(new JavaScriptLibraryInfo
                    {
                        Name = lib.Key,
                        Version = versionMatch.Success ? versionMatch.Groups[1].Value : "unknown",
                        CDNPath = src,
                        Confidence = 0.9
                    });
                    continue;
                }
            }
        }

        // Debug log the content scan results
        if (libraries.Count == 0)
        {
            _logger.LogInformation("No libraries found via CDN paths, scanning content...");
            
            // Add a sample of the content for debugging
            var contentPreview = content.Length > 500 ? content.Substring(0, 500) : content;
            _logger.LogInformation("Content preview: {Preview}", contentPreview);
        }

        return libraries;
    }

    private List<string> DetectMetaTechnologies(string content)
    {
        var technologies = new List<string>();
        
        var metaPatterns = new Dictionary<string, string>
        {
            { "WordPress", @"<meta[^>]*generator[^>]*WordPress" },
            { "Drupal", @"<meta[^>]*generator[^>]*Drupal" },
            { "Joomla", @"<meta[^>]*generator[^>]*Joomla" },
            { "Ghost", @"<meta[^>]*generator[^>]*Ghost" },
            { "Shopify", @"<meta[^>]*generator[^>]*Shopify" },
            { "Wix", @"<meta[^>]*generator[^>]*Wix" },
            { "Webflow", @"<meta[^>]*generator[^>]*Webflow" },
            { "Squarespace", @"<meta[^>]*generator[^>]*Squarespace" }
        };

        foreach (var pattern in metaPatterns)
        {
            if (Regex.IsMatch(content, pattern.Value, RegexOptions.IgnoreCase))
            {
                technologies.Add(pattern.Key);
            }
        }

        return technologies;
    }

    private List<TechnologyFingerprint> DetectTechnologies(string content, Dictionary<string, string> headers)
    {
        var technologies = new List<TechnologyFingerprint>();
        
        // WordPress Detection
        if (content.Contains("wp-content") || content.Contains("wp-includes"))
        {
            technologies.Add(new TechnologyFingerprint
            {
                Name = "WordPress",
                Category = "CMS",
                Version = DetectWordPressVersion(content),
                ConfidenceLevel = "High"
            });
        }
        
        // Cloudflare Detection
        if (headers.ContainsKey("CF-Ray"))
        {
            technologies.Add(new TechnologyFingerprint
            {
                Name = "Cloudflare",
                Category = "CDN",
                Version = headers.GetValueOrDefault("cf-edge-cache", ""),
                ConfidenceLevel = "High"
            });
        }
        
        // Kinsta Detection
        if (headers.Any(h => h.Key.StartsWith("ki-", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add(new TechnologyFingerprint
            {
                Name = "Kinsta",
                Category = "Hosting",
                Version = headers.GetValueOrDefault("ki-edge", ""),
                ConfidenceLevel = "High"
            });
        }

        return technologies;
    }

    private string DetectWordPressVersion(string content)
    {
        var versionMatch = Regex.Match(content, @"meta\s+name=[""']generator[""']\s+content=[""']WordPress\s+([\d.]+)");
        return versionMatch.Success ? versionMatch.Groups[1].Value : "Unknown";
    }
} 