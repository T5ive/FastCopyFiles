using FastCopyFiles.Domain;
using FastCopyFiles.Extension;
using FastCopyFiles.Utility;

namespace FastCopyFiles;

internal class Program
{
    private static AppSetting _appSetting = new();
    private static PathInfo? _pathInfo;

    private static void Main(string[] args)
    {
        if (args.Length == 0)
            Error();

        LoadSettings();
        GetPathInfo(args[0]);

        while (true)
        {
            DisplayChoice();
        }

        // ReSharper disable once FunctionNeverReturns
    }

    #region Load

    private static void Error()
    {
        ConsoleExtensions.WriteLine("============ Fast Copy Files ============", ConsoleColor.Cyan);
        ConsoleExtensions.WriteLine("Have to add arg directory path in shortcut file!!\n\n", ConsoleColor.Red);
        ConsoleExtensions.Write("Press Enter to exit...", ConsoleColor.White);
        Console.ReadKey();
        Environment.Exit(0);
    }

    private static void LoadSettings()
    {
        var settingsPath = Path.Combine(Environment.CurrentDirectory, "settings.json");
        if (!File.Exists(settingsPath))
        {
            _appSetting.Save();
        }
        else
        {
            _appSetting = AppSetting.Load();
        }
    }

    private static void GetPathInfo(string path)
    {
        _pathInfo = _appSetting.ListPath.FirstOrDefault(pathInfo => pathInfo.Path == path || pathInfo.Id == path);

        if (_pathInfo != null) return;

        string? id = null;
        if (!Directory.Exists(path))
        {
            id = path;
            path = AddNew();
        }

        var pathInfo = new PathInfo
        {
            Id = id,
            Path = path
        };

        _appSetting.ListPath.Add(pathInfo);
        _appSetting.Save();
        _pathInfo = _appSetting.ListPath.FirstOrDefault(u => u.Path == path || u.Id == path);
    }

    #region PathInfo Detection

    private static string AddNew()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("============ Fast Copy Files ============", ConsoleColor.Cyan);
        ConsoleExtensions.WriteLine("Can not contains any setup in this directory\nYou must setup new directory\n");
        ConsoleExtensions.Write("Path: ", ConsoleColor.White);
        var path = Console.ReadLine();

        if (!Directory.Exists(path))
        {
            return AddNew();
        }

        return path;
    }

    #endregion PathInfo Detection

    #endregion Load

    #region Main

    private static void DisplayChoice()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("============ Fast Copy Files ============", ConsoleColor.Cyan);
        ConsoleExtensions.WriteLine("0. Copy");
        ConsoleExtensions.WriteLine("1. Get Info");
        ConsoleExtensions.WriteLine("2. Add Extension");
        ConsoleExtensions.WriteLine("3. Add Find and Replace");
        ConsoleExtensions.WriteLine("4. Remove Extension");
        ConsoleExtensions.WriteLine("5. Remove Find and Replace");
        ConsoleExtensions.WriteLine("6. Reset");
        Console.Write("\nChoose: ");

        MakeAction();
    }

    private static void MakeAction()
    {
        var key = Console.ReadLine();
        if (string.IsNullOrEmpty(key) || key.StartsWith("0"))
        {
            Copy();
            return;
        }

        if (key.StartsWith("1"))
        {
            GetInfo();
            return;
        }
        if (key.StartsWith("2"))
        {
            AddExtension();
            return;
        }
        if (key.StartsWith("3"))
        {
            AddFindReplace();
            return;
        }
        if (key.StartsWith("4"))
        {
            RemoveExtension();
            return;
        }
        if (key.StartsWith("5"))
        {
            RemoveFindReplace();
            return;
        }
        if (key.StartsWith("6"))
        {
            Reset();
        }
    }

    #region Action

    private static void Copy()
    {
        var list = Directory.EnumerateFiles(_pathInfo!.Path, "*.*", SearchOption.AllDirectories)
            .Where(file => _pathInfo.Extensions.Any(file.EndsWith))
            .Select(file => new FileInfo(file))
            .ToArray();


        if (list is { Length: > 0 })
        {
            var isReplaces = _pathInfo.Replaces is { Count: > 0 };
            foreach (var result in list)
            {
                var fileName = result.Name;

                if (isReplaces)
                    fileName = _pathInfo.Replaces.Aggregate(fileName, (current, replace) => current.Replace(replace.Find, replace.Replace));

                var destination = Path.Combine(_pathInfo.Path, fileName);
                File.Move(result.FullName, destination, true);
            }
        }

        Environment.Exit(0);
    }

    private static void GetInfo()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("================ Get Info ================", ConsoleColor.Cyan);

        ConsoleExtensions.Write("Id: ", ConsoleColor.Yellow);
        ConsoleExtensions.WriteLine(_pathInfo!.Id!);
        ConsoleExtensions.Write("Path: ", ConsoleColor.Yellow);
        ConsoleExtensions.WriteLine(_pathInfo.Path);

        ConsoleExtensions.WriteLine("\nExtension: ", ConsoleColor.Yellow);
        for (var i = 0; i < _pathInfo!.Extensions.Count; i++)
        {
            var extension = _pathInfo!.Extensions[i];
            ConsoleExtensions.WriteLine(i + 1 + ". " + extension);
        }

        ConsoleExtensions.WriteLine("\nReplace: ", ConsoleColor.Yellow);

        for (var i = 0; i < _pathInfo!.Replaces.Count; i++)
        {
            var replace = _pathInfo!.Replaces[i];
            ConsoleExtensions.Write(i + 1 + ". \"" + replace.Find + "\"");
            ConsoleExtensions.Write(" -> ", ConsoleColor.Magenta);
            ConsoleExtensions.WriteLine("\"" + replace.Replace + "\"");
        }

        ConsoleExtensions.WriteLine("\n=========================================\n\n", ConsoleColor.Cyan);
        ConsoleExtensions.Write("Enter to Go Back...");
        Console.ReadLine();
    }

    private static void AddExtension()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("================== Add Extension ==================", ConsoleColor.Cyan);

        ConsoleExtensions.WriteLine("Enter Empty value to Go Back\n", ConsoleColor.Yellow);

        Console.Write("Add Extension: ");

        var key = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(key)) return;

        if (key.StartsWith("."))
            key = key.Replace(".", "");

        _pathInfo!.Extensions.Add(key);
        _appSetting.Save();
    }

    private static void AddFindReplace()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("================== Add Find and Replace ==================", ConsoleColor.Cyan);
        ConsoleExtensions.WriteLine("Enter Empty value to Go Back\n", ConsoleColor.Yellow);

        Console.Write("Add Find: ");

        var find = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(find))
            return;

        Console.Write("Add Replace: ");

        var replace = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(replace))
            replace = "";

        var replaceInfo = new ReplaceInfo
        {
            Find = find,
            Replace = replace
        };

        _pathInfo!.Replaces.Add(replaceInfo);
        _appSetting.Save();
    }

    private static void RemoveExtension()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("================== Remove Extension ==================", ConsoleColor.Cyan);

        ConsoleExtensions.WriteLine("\nExtension: ", ConsoleColor.Yellow);
        for (var i = 0; i < _pathInfo!.Extensions.Count; i++)
        {
            var extension = _pathInfo!.Extensions[i];
            ConsoleExtensions.WriteLine(i + 1 + ". " + extension);
        }

        ConsoleExtensions.WriteLine("Enter Empty value to Go Back\n", ConsoleColor.Yellow);

        Console.Write("Choice Extension: ");

        var key = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(key)) return;

        var isNum = int.TryParse(key, out var id);
        if (isNum)
        {
            if (id <= _pathInfo!.Extensions.Count)
            {
                id -= 1;
                _pathInfo.Extensions.RemoveAt(id);
                _appSetting.Save();
                return;
            }
        }

        RemoveExtension();
    }

    private static void RemoveFindReplace()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("================== Remove Find and Replace ==================", ConsoleColor.Cyan);

        ConsoleExtensions.WriteLine("\nReplace: ", ConsoleColor.Yellow);

        for (var i = 0; i < _pathInfo!.Replaces.Count; i++)
        {
            var replace = _pathInfo!.Replaces[i];
            ConsoleExtensions.Write(i + 1 + ". \"" + replace.Find + "\"");
            ConsoleExtensions.Write(" -> ", ConsoleColor.Magenta);
            ConsoleExtensions.WriteLine("\"" + replace.Replace + "\"");
        }

        ConsoleExtensions.WriteLine("Enter Empty value to Go Back\n", ConsoleColor.Yellow);

        Console.Write("Choice Find and Replace: ");

        var key = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(key)) return;

        var isNum = int.TryParse(key, out var id);
        if (isNum)
        {
            if (id <= _pathInfo!.Replaces.Count)
            {
                id -= 1;
                _pathInfo.Replaces.RemoveAt(id);
                _appSetting.Save();
                return;
            }
        }

        RemoveExtension();
    }

    private static void Reset()
    {
        Console.Clear();
        ConsoleExtensions.WriteLine("================== Reset ==================", ConsoleColor.Cyan);

        ConsoleExtensions.WriteLine("Enter Empty value to Go Back", ConsoleColor.Yellow);
        ConsoleExtensions.WriteLine("Enter Y to reset\n", ConsoleColor.Red);

        Console.Write("Enter: ");
        var key = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(key)) return;

        if (key.Equals("y", StringComparison.CurrentCultureIgnoreCase))
        {
            _pathInfo!.Extensions = new List<string>();
            _pathInfo.Replaces = new List<ReplaceInfo>();
            _appSetting.Save();
        }
    }

    #endregion Action

    #endregion Main
}