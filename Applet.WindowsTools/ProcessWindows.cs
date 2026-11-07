using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Applets.WindowsTools;

internal enum ProcessWindowAction { Minimize, Restore, Toggle }
internal readonly record struct ProcessWindowTarget(nint Handle, int ProcessId);

internal interface IProcessWindowSystem
{
    IReadOnlyList<ProcessWindowTarget> Find(string processName);
    bool IsCurrent(ProcessWindowTarget target);
    bool IsMinimized(nint window);
    void Request(nint window, ProcessWindowAction action);
}

internal sealed class ProcessWindows(IProcessWindowSystem system)
{
    internal const string TypeId = "at365.windows-tools.process-window";
    internal static readonly (string Suffix, string Title, ProcessWindowAction Action)[] RemoteDesktopCommands = [
        ("rdp-minimize", "リモートデスクトップを最小化", ProcessWindowAction.Minimize),
        ("rdp-restore", "リモートデスクトップを最小化から復元", ProcessWindowAction.Restore),
        ("rdp-toggle", "リモートデスクトップを最小化／復元", ProcessWindowAction.Toggle)
    ];

    internal static string NormalizeName(string value)
    {
        var name = value.Trim();
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name = name[..^4];
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." ||
            name.IndexOfAny(['\\', '/', ':', '*', '?', '"', '<', '>', '|']) >= 0 || name.Any(char.IsControl))
            throw new ArgumentException("対象プロセスには、パスやワイルドカードを含めずプロセス名を入力してください。");
        return name;
    }

    internal static (string Name, ProcessWindowAction Action) Parse(JsonElement config)
    {
        if (config.ValueKind != JsonValueKind.Object ||
            config.EnumerateObject().Any(p => p.Name is not ("processName" or "action")) ||
            !config.TryGetProperty("processName", out var name) || name.ValueKind != JsonValueKind.String ||
            !config.TryGetProperty("action", out var action) || action.ValueKind != JsonValueKind.String)
            throw new ArgumentException("対象プロセスと操作を指定してください。");
        var mode = action.GetString() switch {
            "minimize" => ProcessWindowAction.Minimize,
            "restore" => ProcessWindowAction.Restore,
            "toggle" => ProcessWindowAction.Toggle,
            _ => throw new ArgumentException("操作は最小化・復元・最小化／復元から選択してください。")
        };
        return (NormalizeName(name.GetString()!), mode);
    }

    public void Execute(string processName, ProcessWindowAction action, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var name = NormalizeName(processName);
        if (!Enum.IsDefined(action)) throw new ArgumentException("操作が不正です。");
        foreach (var target in system.Find(name)) {
            token.ThrowIfCancellationRequested();
            if (!system.IsCurrent(target)) continue;
            var requested = action == ProcessWindowAction.Toggle
                ? (system.IsMinimized(target.Handle) ? ProcessWindowAction.Restore : ProcessWindowAction.Minimize)
                : action;
            token.ThrowIfCancellationRequested();
            if (system.IsCurrent(target)) system.Request(target.Handle, requested);
        }
    }
}

internal sealed class NativeProcessWindows : IProcessWindowSystem
{
    public IReadOnlyList<ProcessWindowTarget> Find(string processName)
    {
        var result = new List<ProcessWindowTarget>();
        var processes = Process.GetProcessesByName(processName);
        try {
            foreach (var process in processes) {
                try {
                    var handle = process.MainWindowHandle;
                    if (handle != 0) result.Add(new(handle, process.Id));
                } catch (InvalidOperationException) {
                    // A process that exited during enumeration is no longer a target.
                }
            }
        } finally {
            foreach (var process in processes) process.Dispose();
        }
        return result;
    }

    public bool IsCurrent(ProcessWindowTarget target)
    {
        GetWindowThreadProcessId(target.Handle, out var processId);
        return processId == target.ProcessId && IsWindowVisible(target.Handle);
    }
    public bool IsMinimized(nint window) => IsIconic(window);
    public void Request(nint window, ProcessWindowAction action)
    {
        // Match Invoke-MinimizeWindow: SW_MINIMIZE / SW_RESTORE, with foreground requested on restore.
        if (!ShowWindowAsync(window, action == ProcessWindowAction.Minimize ? 6 : 9))
            throw new InvalidOperationException("対象ウィンドウへの最小化・復元要求を送信できませんでした。");
        if (action == ProcessWindowAction.Restore) SetForegroundWindow(window);
    }
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint processId);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")] private static extern bool IsIconic(nint window);
    [DllImport("user32.dll")] private static extern bool ShowWindowAsync(nint window, int command);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(nint window);
}
