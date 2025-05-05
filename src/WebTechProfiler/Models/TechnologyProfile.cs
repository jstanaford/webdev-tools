namespace WebTechProfiler.Models;

public class TechnologyProfile
{
    public string Url { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; }
    public List<TechnologyFingerprint> DetectedTechnologies { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public string ServerType { get; set; } = string.Empty;
    public List<JavaScriptLibraryInfo> JavaScriptLibraries { get; set; } = new();
    public List<string> MetaTechnologies { get; set; } = new();
}

public class TechnologyFingerprint
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ConfidenceLevel { get; set; } = string.Empty;
} 