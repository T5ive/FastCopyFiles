using FastCopyFiles.Utility;

namespace FastCopyFiles.Domain;

public class AppSetting : JsonHelper<AppSetting>
{
    public List<PathInfo> ListPath { get; set; } = new();
}