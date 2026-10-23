using System.Reflection;
using System.Text.Json;

namespace Applets.WindowsTools;

internal static class CommandDescriptions
{
    public static IReadOnlyDictionary<string, string> All { get; } = Read();
    private static IReadOnlyDictionary<string, string> Read()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowsTools.CommandDescriptions")!;
        return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
    }
}
