using System.Reflection;
using System.Text.Json;

namespace Applets.WindowsTools;

internal sealed record WindowsProcessCommand(string Id, string Title, string Description,
    string? Executable = null, string[]? Arguments = null, string? Script = null)
{
    public object CreateRequest() => Script is not null
        ? new { mode = "powershell", script = Script, showConsole = false }
        : new {
            mode = "executable",
            executable = Path.Combine(Environment.SystemDirectory, Executable!),
            arguments = Arguments!,
            showConsole = false
        };
}

internal static class WindowsProcessCommands
{
    public static IReadOnlyList<WindowsProcessCommand> All { get; } = Read();
    private static IReadOnlyList<WindowsProcessCommand> Read()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowsTools.ProcessCommands")!;
        return JsonSerializer.Deserialize<WindowsProcessCommand[]>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
