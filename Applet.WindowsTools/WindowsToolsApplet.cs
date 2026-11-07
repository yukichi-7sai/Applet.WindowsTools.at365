using AppDock.SDK;

namespace Applets.WindowsTools;

public sealed class WindowsToolsApplet : IAppDockExtension
{
    private readonly IWindowsPlatform platform;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly DisplayCountdown display = new();
    private readonly IdleLockPolicy idle = new();
    private IExtensionContext context = null!;
    private IDisposable? schedule, settingsSubscription;
    private bool active, autoLockEnabled;
    private int displaySeconds = 5, lockMinutes = 360;
    private static readonly (string Suffix, string Title, MoveCommand Command)[] WindowCommands = [
        ("left", "ウィンドウを左へ移動", MoveCommand.Left),
        ("right", "ウィンドウを右へ移動", MoveCommand.Right),
        ("full-height", "ウィンドウを上下いっぱいに広げる", MoveCommand.FullHeight),
        ("center", "ウィンドウを画面中央へ移動", MoveCommand.Center),
        ("minimize", "ウィンドウを最小化", MoveCommand.Minimize),
        ("maximize", "ウィンドウを最大化", MoveCommand.Maximize),
        ("restore", "ウィンドウを元のサイズに戻す", MoveCommand.Restore),
        ("toggle-maximize", "ウィンドウを最大化／元に戻す", MoveCommand.ToggleMaximize),
        ("close", "ウィンドウを閉じる", MoveCommand.Close)
    ];
    public WindowsToolsApplet() : this(new WindowsPlatform()) { }
    internal WindowsToolsApplet(IWindowsPlatform platform) => this.platform = platform;

    public async Task ActivateAsync(IExtensionContext context, CancellationToken token)
    {
        this.context = context;
        active = true;
        idle.Reset(platform.Uptime);
        await RefreshSettings(token);
        settingsSubscription = context.Settings.OnChanged(RefreshSettings);
        schedule = context.Scheduler.Every(TimeSpan.FromSeconds(1), Tick);
    }

    private async Task RefreshSettings(CancellationToken token)
    {
        await gate.WaitAsync(token);
        try
        {
            if (!active) return;
            var commands = new List<CommandRegistration> {
                Command("open-folder", "最前面アプリの実行ファイルのフォルダーを開く", ct => { platform.OpenForegroundFolder(); return Task.CompletedTask; }),
                Command("lock-windows", "Windowsをロックする", ct => { platform.Lock(); return Task.CompletedTask; },
                    "Windowsを直ちにロックします。開いているアプリは終了しません。ロックの解除にはWindowsへのサインインが必要です。"),
                Command("display-off", "ディスプレイの消灯を予約", ct => {
                    display.Schedule(platform.Uptime, displaySeconds); return Task.CompletedTask;
                }),
                Command("cancel-display-off", "ディスプレイの消灯予約を取り消す", ct => {
                    display.Cancel(); return Task.CompletedTask;
                }),
                Command("toggle-auto-lock", "無操作時の自動ロックを切り替える", async ct => {
                    var enabled = !autoLockEnabled;
                    await context.Settings.SetAsync("autoLockEnabled", enabled, ct);
                    autoLockEnabled = enabled; idle.Reset(platform.Uptime);
                })
            };
            foreach (var command in WindowCommands)
                commands.Add(Command(command.Suffix, command.Title, ct => {
                    platform.ExecuteWindow(command.Command, ct); return Task.CompletedTask;
                }));
            foreach (var shortcut in WindowsShortcuts.All)
                commands.Add(Command(shortcut.Suffix, shortcut.Title,
                    ct => context.AppDock.CallAsync("input.sendKeys", new { keys = shortcut.Chord.ToShortcut() }, ct), shortcut.CommandDescription));
            foreach (var command in WindowsProcessCommands.All)
                commands.Add(Command(command.Id, command.Title,
                    ct => context.AppDock.CallAsync("processes.start", command.CreateRequest(), ct), command.Description));
            await context.Commands.ReplaceAsync(commands, token);
            displaySeconds = Math.Clamp(context.Settings.Get("displayOffSeconds", 5), 1, 3600);
            var enabled = context.Settings.Get("autoLockEnabled", false);
            var minutes = Math.Clamp(context.Settings.Get("autoLockMinutes", 360), 1, 1440);
            if (enabled != autoLockEnabled || minutes != lockMinutes) idle.Reset(platform.Uptime);
            autoLockEnabled = enabled;
            lockMinutes = minutes;

        }
        finally { gate.Release(); }
    }

    private CommandRegistration Command(string suffix, string title, Func<CancellationToken, Task> action, string? description = null) =>
        new(context.ExtensionId + "." + suffix, title, async token => {
            await gate.WaitAsync(token);
            try { token.ThrowIfCancellationRequested(); if (active) await action(token); }
            finally { gate.Release(); }
        }) { Description = description ?? CommandDescriptions.All[suffix] };

    private async Task Tick(CancellationToken token)
    {
        await gate.WaitAsync(token);
        try
        {
            token.ThrowIfCancellationRequested();
            if (!active) return;
            if (display.TakeDue(platform.Uptime))
            {
                platform.TurnOffDisplay();

            }
            token.ThrowIfCancellationRequested();
            if (autoLockEnabled && idle.TakeDue(platform.Uptime, platform.LastInputTimestamp(), lockMinutes))
                platform.Lock();
        }
        finally { gate.Release(); }
    }

    public async Task DeactivateAsync(CancellationToken token)
    {
        settingsSubscription?.Dispose(); settingsSubscription = null;
        schedule?.Dispose(); schedule = null;
        await gate.WaitAsync(token);
        try { active = false; display.Cancel(); }
        finally { gate.Release(); }
    }
}
