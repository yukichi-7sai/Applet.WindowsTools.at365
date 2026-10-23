param([Parameter(Mandatory=$true)][string]$OutputDirectory, [ValidateSet('shutdown','terminal')][string]$Kind = 'shutdown')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class ShutdownDialogProbe {
    public delegate bool Callback(IntPtr window, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumWindows(Callback callback, IntPtr data);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr window, StringBuilder text, int size);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr window, StringBuilder text, int size);
    [DllImport("user32.dll")] public static extern IntPtr GetDlgItem(IntPtr window, int id);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string kind, string title);
    [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr window);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window, IntPtr context, uint flags);
    [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    public static string Title(IntPtr window) { var text=new StringBuilder(256); GetWindowText(window,text,text.Capacity); return text.ToString(); }
    public static IntPtr[] Find(bool terminal) {
        var result=new List<IntPtr>();
        EnumWindows((window,data)=>{
            var kind=new StringBuilder(256); GetClassName(window,kind,kind.Capacity);
            var title=Title(window);
            if(IsWindowVisible(window) && (terminal ? kind.ToString()=="CASCADIA_HOSTING_WINDOW_CLASS" :
                kind.ToString()=="#32770" && (title=="Shut Down Windows" || (title.Contains("Windows") && title.Contains("シャットダウン"))))) result.Add(window);
            return true;
        },IntPtr.Zero);
        return result.ToArray();
    }
}
'@
$taskTerminal = $Kind -eq 'terminal'
$taskPrefix = if ($taskTerminal) { 'terminal' } else { 'dialog' }
$taskExisting = @([ShutdownDialogProbe]::Find($taskTerminal) | ForEach-Object { $_.ToInt64() })
[IO.File]::WriteAllText((Join-Path $OutputDirectory ($taskPrefix + '-probe.ready')), 'ready')
$taskDeadline = [DateTime]::UtcNow.AddSeconds(25)
$taskFound = [IntPtr]::Zero
while ([DateTime]::UtcNow -lt $taskDeadline -and $taskFound -eq [IntPtr]::Zero) {
    foreach ($taskWindow in [ShutdownDialogProbe]::Find($taskTerminal)) {
        if ($taskWindow.ToInt64() -notin $taskExisting) { $taskFound = $taskWindow; break }
    }
    if ($taskFound -eq [IntPtr]::Zero) { Start-Sleep -Milliseconds 100 }
}
if ($taskFound -eq [IntPtr]::Zero) { throw ('The new ' + $Kind + ' window did not appear.') }
try {
    Start-Sleep -Milliseconds 500
    if ($taskTerminal) {
        $taskResult = @{ terminalWindow=$true; newWindow=$true }
    } else {
    if ([ShutdownDialogProbe]::GetDlgItem($taskFound, 1) -eq [IntPtr]::Zero -or
        [ShutdownDialogProbe]::GetDlgItem($taskFound, 2) -eq [IntPtr]::Zero -or
        [ShutdownDialogProbe]::FindWindowEx($taskFound, [IntPtr]::Zero, 'ComboBox', $null) -eq [IntPtr]::Zero) {
        throw 'The shutdown choice, OK and Cancel controls are missing.'
    }
    [void][ShutdownDialogProbe]::SetThreadDpiAwarenessContext([IntPtr](-4))
    $taskRect = New-Object ShutdownDialogProbe+Rect
    if (![ShutdownDialogProbe]::GetWindowRect($taskFound, [ref]$taskRect)) { throw 'Cannot read dialog bounds.' }
    $taskBitmap = New-Object System.Drawing.Bitmap ($taskRect.Right-$taskRect.Left), ($taskRect.Bottom-$taskRect.Top)
    $taskGraphics = [Drawing.Graphics]::FromImage($taskBitmap)
    try {
        $taskContext = $taskGraphics.GetHdc()
        try {
            if (![ShutdownDialogProbe]::PrintWindow($taskFound, $taskContext, 2)) { throw 'Cannot capture the shutdown dialog.' }
        } finally { $taskGraphics.ReleaseHdc($taskContext) }
        $taskBitmap.Save((Join-Path $OutputDirectory 'shutdown-dialog.png'))
    } finally { $taskGraphics.Dispose(); $taskBitmap.Dispose() }
    $taskResult = @{ title=[ShutdownDialogProbe]::Title($taskFound); choice=$true; okButton=$true; cancelButton=$true }
    }
} finally {
    # WM_CLOSE cancels only the new matching dialog; never press OK or send Alt+F4.
    [void][ShutdownDialogProbe]::PostMessage($taskFound, 0x10, [IntPtr]::Zero, [IntPtr]::Zero)
}
$taskDeadline = [DateTime]::UtcNow.AddSeconds(5)
while ([ShutdownDialogProbe]::IsWindow($taskFound) -and [DateTime]::UtcNow -lt $taskDeadline) { Start-Sleep -Milliseconds 100 }
if ([ShutdownDialogProbe]::IsWindow($taskFound)) { throw 'The dialog did not close after cancellation.' }
$taskResult.cancelled = $true
$taskResultFile = if ($taskTerminal) { 'terminal-window.json' } else { 'shutdown-dialog.json' }
$taskResult | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory $taskResultFile) -Encoding UTF8
