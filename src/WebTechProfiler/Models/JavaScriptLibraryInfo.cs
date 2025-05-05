namespace WebTechProfiler.Models;

public class JavaScriptLibraryInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string CDNPath { get; set; } = string.Empty;
    public double Confidence { get; set; }
} 