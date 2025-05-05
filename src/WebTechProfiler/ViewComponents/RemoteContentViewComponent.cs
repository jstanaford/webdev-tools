using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebTechProfiler.Models;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using System.Web;

namespace WebTechProfiler.ViewComponents
{
    public class RemoteContentViewComponent : ViewComponent
    {
        private readonly ILogger<RemoteContentViewComponent> _logger;

        public RemoteContentViewComponent(ILogger<RemoteContentViewComponent> logger)
        {
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string ip, string url)
        {
            try
            {
                var uri = new Uri("http://" + url);
                var domain = uri.Host;
                var pathAndQuery = uri.PathAndQuery;

                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = ValidateServerCertificate
                };

                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                var content = await FetchContentWithRedirects(client, domain, ip, pathAndQuery);
                var processedContent = ProcessHtmlContent(content, domain, ip);
                return View("Default", processedContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching content for {Url} at {IP}", url, ip);
                return View("Error", new ErrorViewModel 
                { 
                    Message = $"Failed to load content: {ex.Message}"
                });
            }
        }

        private async Task<string> FetchContentWithRedirects(HttpClient client, string domain, string ip, string pathAndQuery, int maxRedirects = 10)
        {
            var currentUrl = $"http://{ip}{pathAndQuery}";
            var redirectCount = 0;

            while (redirectCount < maxRedirects)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, currentUrl);
                request.Headers.Host = domain;

                _logger.LogInformation("Requesting {Url} with Host: {Host}", currentUrl, domain);
                
                var response = await client.SendAsync(request);
                _logger.LogInformation("Received status code: {StatusCode}", response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently || 
                    response.StatusCode == System.Net.HttpStatusCode.Found || 
                    response.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    var location = response.Headers.Location;
                    if (location == null)
                    {
                        throw new Exception("Redirect location was null");
                    }

                    // Handle relative redirects
                    if (!location.IsAbsoluteUri)
                    {
                        location = new Uri(new Uri($"http://{ip}"), location);
                    }

                    // Keep using our IP but switch protocols if needed
                    var builder = new UriBuilder(location)
                    {
                        Host = ip,
                        Port = -1 // Use default port for protocol
                    };
                    currentUrl = builder.Uri.ToString();

                    redirectCount++;
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Server returned {response.StatusCode}");
                }

                return await response.Content.ReadAsStringAsync();
            }

            throw new Exception("Too many redirects");
        }

        private bool ValidateServerCertificate(
            HttpRequestMessage request,
            X509Certificate2 certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            // Accept any certificate since we're using IP-based requests
            return true;
        }

        private string ProcessHtmlContent(string html, string domain, string ip)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Process anchor tags
            var anchors = doc.DocumentNode.SelectNodes("//a[@href]");
            if (anchors != null)
            {
                foreach (var anchor in anchors)
                {
                    var href = anchor.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(href)) continue;

                    try
                    {
                        var newHref = ProcessUrl(href, domain, ip);
                        if (newHref != href)
                        {
                            anchor.SetAttributeValue("href", newHref);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing URL {Url}", href);
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        private string ProcessUrl(string url, string domain, string ip)
        {
            // Skip if it's a fragment or javascript
            if (url.StartsWith("#") || url.StartsWith("javascript:")) return url;

            try
            {
                Uri uri;
                // Handle relative URLs
                if (!url.Contains("://"))
                {
                    if (url.StartsWith("//"))
                    {
                        url = "http:" + url;
                    }
                    else if (url.StartsWith("/"))
                    {
                        url = $"http://{domain}{url}";
                    }
                    else
                    {
                        url = $"http://{domain}/{url}";
                    }
                }

                uri = new Uri(url);

                // Only process URLs for the same domain
                if (uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase))
                {
                    var path = uri.PathAndQuery;
                    if (path == "/") path = "";
                    return $"/host-viewer/{ip}?url={HttpUtility.UrlEncode($"{domain}{path}")}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing URL {Url}", url);
            }

            return url;
        }
    }
} 