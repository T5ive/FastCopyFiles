using System.Text;

namespace FastCopyFiles.Utility;

public static class Helper
{
    public static string GetPattern(List<string> listExtension)
    {
        var result = new StringBuilder();
        foreach (var extension in listExtension)
        {
            result.Append("*." + extension + "|");
        }
        return result.ToString().EndsWith("|") ?
            result.Remove(result.Length - 1, 1).ToString() :
            result.ToString();
    }
}