using AppDock.SDK;
using KeyChord = AppDock.SDK.InputKeyChord;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Applets.WindowsTools;

internal interface IWindowsPlatform
{
    long Uptime { get; }
    uint LastInputTimestamp();
    void OpenForegroundFolder();
    void TurnOffDisplay();
    void Lock();
    void ExecuteWindow(MoveCommand command, CancellationToken token);
    Task SendAsync(KeyChord chord, CancellationToken token);
}

internal sealed class WindowsPlatform : IWindowsPlatform
{
    internal IAppDockService? AppDock { get; set; }
    public long Uptime => Environment.TickCount64;
    public uint LastInputTimestamp()
    {
        var info = new LastInput { Size = (uint)Marshal.SizeOf<LastInput>() };
        if (!GetLastInputInfo(ref info)) throw new Win32Exception(Marshal.GetLastWin32Error());
        return info.Time;
    }
    public void OpenForegroundFolder()
    {
        var window = Target();
        GetWindowThreadProcessId(window, out var pid);
        using var process = Process.GetProcessById((int)pid);
        var file = process.MainModule?.FileName;
        var directory = Path.GetDirectoryName(file);
        if (string.IsNullOrEmpty(directory)) throw new InvalidOperationException("対象アプリの実行ファイルのフォルダーを取得できませんでした。");
        var start = new ProcessStartInfo("explorer.exe") { UseShellExecute = false };
        start.ArgumentList.Add(directory);
        using var explorer = Process.Start(start);
    }
    public void TurnOffDisplay()
    {
        if (!PostMessage(new nint(0xFFFF), 0x0112, 0xF170, 2)) throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    public void Lock()
    {
        if (!LockWorkStation()) throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    public async Task SendAsync(KeyChord chord, CancellationToken token)
    {
        if (AppDock is null) throw new InvalidOperationException("AppDock入力サービスが未接続です。");
        await AppDock.CallAsync("input.sendKeys", new { keys = chord.ToShortcut() }, token);
    }
    public void ExecuteWindow(MoveCommand command, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // DPI context must be restored on the same thread, without an await.
        using var dpi = NativeWindows.PhysicalCoordinates();
        new WindowMover(new NativeWindows()).Execute(command, token);
    }
    private static nint Target()
    {
        var window = GetForegroundWindow();
        if (window == 0 || window == GetDesktopWindow() || window == GetShellWindow() || !IsWindowVisible(window))
            throw new InvalidOperationException("操作対象のアプリを最前面にしてください。");
        return window;
    }
    [StructLayout(LayoutKind.Sequential)] private struct LastInput { public uint Size, Time; }
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern nint GetDesktopWindow();
    [DllImport("user32.dll")] private static extern nint GetShellWindow();
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetLastInputInfo(ref LastInput info);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool LockWorkStation();
    [DllImport("user32.dll", SetLastError = true)] private static extern bool PostMessage(nint window, uint message, nint wParam, nint lParam);
}
