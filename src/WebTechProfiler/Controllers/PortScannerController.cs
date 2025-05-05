using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace WebTechProfiler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortScannerController : ControllerBase
{
    private readonly ILogger<PortScannerController> _logger;
    private static readonly Dictionary<int, string> commonPorts = new()
    {
        {7, "Echo Protocol"},
        {9, "Wake-on-LAN"},
        {13, "Daytime Protocol"},
        {21, "FTP (File Transfer Protocol)"},
        {22, "SSH (Secure Shell)"},
        {23, "Telnet"},
        {25, "SMTP (Simple Mail Transfer Protocol)"},
        {26, "RSFTP"},
        {37, "Time Protocol"},
        {53, "DNS (Domain Name System)"},
        {79, "Finger Protocol"},
        {80, "HTTP (Web)"},
        {81, "TorPark Onion Routing"},
        {88, "Kerberos"},
        {106, "POP3PW"},
        {110, "POP3 (Post Office Protocol v3)"},
        {111, "RPCBind"},
        {113, "Ident"},
        {119, "NNTP (Network News Transfer Protocol)"},
        {135, "MSRPC"},
        {139, "NetBIOS"},
        {143, "IMAP (Internet Message Access Protocol)"},
        {144, "NewS"},
        {179, "BGP (Border Gateway Protocol)"},
        {199, "SMUX"},
        {389, "LDAP (Lightweight Directory Access Protocol)"},
        {427, "SLP (Service Location Protocol)"},
        {443, "HTTPS (HTTP over SSL/TLS)"},
        {444, "SNPP"},
        {445, "Microsoft-DS (Active Directory)"},
        {465, "SMTPS (SMTP over SSL)"},
        {513, "Rlogin"},
        {514, "RSH (Remote Shell)"},
        {515, "LPD (Line Printer Daemon)"},
        {543, "Klogin"},
        {544, "Kshell"},
        {548, "AFP (Apple Filing Protocol)"},
        {554, "RTSP (Real Time Streaming Protocol)"},
        {587, "SMTP (Submission)"},
        {631, "IPP (Internet Printing Protocol)"},
        {646, "LDP (Label Distribution Protocol)"},
        {873, "Rsync"},
        {990, "FTPS (FTP over SSL)"},
        {993, "IMAPS (IMAP over SSL)"},
        {995, "POP3S (POP3 over SSL)"},
        {1025, "NFS or IIS"},
        {1026, "Windows Messenger"},
        {1027, "Windows IPv6"},
        {1028, "Windows DCOM"},
        {1029, "Microsoft DCOM"},
        {1110, "NFSD-keepalive"},
        {1433, "MSSQL"},
        {1720, "H.323"},
        {1723, "PPTP (VPN)"},
        {1755, "MMS (Microsoft Media Services)"},
        {1900, "SSDP (UPnP)"},
        {2000, "Cisco SCCP"},
        {2001, "CAPTAN"},
        {2049, "NFS (Network File System)"},
        {2121, "CCProxy"},
        {2717, "Microsoft Terminal Server"},
        {3000, "Ruby on Rails"},
        {3128, "Squid Proxy"},
        {3306, "MySQL"},
        {3389, "RDP (Remote Desktop Protocol)"},
        {3986, "MAPPER"},
        {4444, "Metasploit"},
        {4899, "Radmin"},
        {5000, "UPnP"},
        {5009, "Microsoft Windows Media"},
        {5051, "ita-agent"},
        {5060, "SIP"},
        {5101, "ADMDOG"},
        {5190, "AOL Instant Messenger"},
        {5357, "WSDAPI"},
        {5432, "PostgreSQL"},
        {5631, "PCAnywhere"},
        {5666, "NRPE (Nagios)"},
        {5800, "VNC Remote Desktop"},
        {5900, "VNC Server"},
        {5985, "WinRM"},
        {5986, "WinRM over HTTPS"},
        {6000, "X11"},
        {6001, "X11:1"},
        {6646, "McAfee"},
        {7070, "RealAudio"},
        {8000, "HTTP Alternate"},
        {8008, "HTTP Alternate"},
        {8009, "AJP (Apache JServ Protocol)"},
        {8080, "HTTP Proxy"},
        {8081, "HTTP Alternate"},
        {8443, "HTTPS Alternate"},
        {8888, "HTTP Alternate"},
        {9100, "PDL Data Stream"},
        {9999, "Crypto"},
        {10000, "Webmin"},
        {32768, "Filenet TMS"},
        {49152, "Windows RPC"},
        {49153, "Windows RPC"},
        {49154, "Windows RPC"},
        {49155, "Windows RPC"},
        {49156, "Windows RPC"},
        {49157, "Windows RPC"}
    };

    public PortScannerController(ILogger<PortScannerController> logger)
    {
        _logger = logger;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> ScanPorts([FromBody] ScanRequest request)
    {
        if (string.IsNullOrEmpty(request.Url))
        {
            return BadRequest("URL is required");
        }

        try
        {
            // Strip protocol and path
            request.Url = Regex.Replace(request.Url, @"^(https?://)", "").Split('/')[0];
            var results = await ScanCommonPorts(request.Url);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning ports for {Url}", request.Url);
            return StatusCode(500, "Error scanning ports: " + ex.Message);
        }
    }

    private async Task<List<PortScanResult>> ScanCommonPorts(string url)
    {
        var tasks = commonPorts.Select(async port =>
        {
            var isOpen = await CheckPortAsync(url, port.Key);
            return new PortScanResult
            {
                Port = port.Key,
                Service = port.Value,
                IsOpen = isOpen
            };
        });

        return (await Task.WhenAll(tasks)).OrderBy(r => r.Port).ToList();
    }

    private async Task<bool> CheckPortAsync(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            var timeoutTask = Task.Delay(1000);
            var connectTask = client.ConnectAsync(host, port);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            return completedTask != timeoutTask;
        }
        catch
        {
            return false;
        }
    }

    [HttpPost("direct-scan")]
    public async Task<IActionResult> DirectScan([FromBody] DirectScanRequest request)
    {
        if (string.IsNullOrEmpty(request.Url))
        {
            return BadRequest("URL is required");
        }

        if (request.Port < 1 || request.Port > 65535)
        {
            return BadRequest("Port must be between 1 and 65535");
        }

        try
        {
            request.Url = Regex.Replace(request.Url, @"^(https?://)", "").Split('/')[0];
            var isOpen = await CheckPortAsync(request.Url, request.Port);
            
            return Ok(new DirectScanResult
            {
                Port = request.Port,
                Service = GetCommonServiceName(request.Port),
                IsOpen = isOpen
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning port {Port} for {Url}", request.Port, request.Url);
            return StatusCode(500, "Error scanning port: " + ex.Message);
        }
    }

    private string GetCommonServiceName(int port)
    {
        return commonPorts.ContainsKey(port) ? commonPorts[port] : "Unknown Service";
    }

    public class ScanRequest
    {
        public string Url { get; set; } = string.Empty;
    }

    public class PortScanResult
    {
        public int Port { get; set; }
        public string Service { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
    }

    public class DirectScanRequest
    {
        public string Url { get; set; } = string.Empty;
        public int Port { get; set; }
    }

    public class DirectScanResult
    {
        public int Port { get; set; }
        public string Service { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
    }
} 