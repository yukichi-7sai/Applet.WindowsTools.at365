using System.Reflection;
using System.Text.Json;
using AppDock.SDK;

namespace Applets.WindowsTools;

internal sealed record WindowsShortcut(string Id, string Title, string Keys, string Description)
{
    public string Suffix => "windows." + Id;
    public InputKeyChord Chord => InputKeyChord.Parse(Keys);
    public string CommandDescription => $"送信キー: {Keys}。{Description}";
}

internal static class WindowsShortcuts
{
    public static IReadOnlyList<WindowsShortcut> All { get; } = Read();
    private static IReadOnlyList<WindowsShortcut> Read()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowsTools.Shortcuts")!;
        return JsonSerializer.Deserialize<WindowsShortcut[]>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
