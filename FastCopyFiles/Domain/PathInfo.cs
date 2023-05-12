namespace FastCopyFiles.Domain;

public class PathInfo
{
    public string? Id { get; set; }
    public string Path { get; set; }
    public List<string> Extensions { get; set; } = new();

    public List<ReplaceInfo> Replaces { get; set; } = new();
}