using KeyChord = AppDock.SDK.InputKeyChord;
using System.Text.Json;
using AppDock.SDK;
using Applets.WindowsTools;

internal static class Program
{
    private static int FixedCount => 13 + WindowsShortcuts.All.Count;
    [STAThread]
    private static int Main(string[] args)
    {
        if (args is ["--native-smoke", var runner, var assembly]) return NativeSmoke.Run(runner, assembly);
        if (args is ["--gesture-native-smoke", var gestureRunner, var gestureAssembly]) return NativeSmoke.Run(gestureRunner, gestureAssembly, capturedTarget: true);
        if (args is ["--protocol-smoke", var protocolRunner, var protocolAssembly]) return NativeSmoke.Protocol(protocolRunner, protocolAssembly);
        return Run().GetAwaiter().GetResult();
    }
    private static async Task<int> Run()
    {
        var tests = new List<(string, Func<Task>)>();
        WindowMovementTests.AddTo(tests);
        void Test(string name, Action test) => tests.Add((name, () => { test(); return Task.CompletedTask; }));
        Test("shortcut grammar and virtual keys", () => {
            var chord = KeyChord.Parse("Ctrl+Shift+T");
            Check(chord.Key == 'T' && chord.Modifiers.SequenceEqual(new ushort[] { 0x11, 0x10 }));
            Check(KeyChord.Parse("Win+Left").Extended);
            Check(KeyChord.Parse("F24").Key == 0x87);
            Check(KeyChord.Parse("Alt+F4").Key == 0x73);
        });
        Test("reject unknown, repeated and empty keys", () => {
            foreach (var keys in new[] { "", "Ctrl+", "Ctrl+Ctrl+A", "Ctrl+Control+A", "F25", "%{F4}", "Ctrl+A+B", "Text: hello" })
                Throws(() => KeyChord.Parse(keys));
        });
        Test("system keys and symbolic keys round trip without binding to a keyboard layout", () => {
            foreach (var keys in new[] { "Win", "Win+Pause", "Win+PrintScreen", "Win+Period", "Win+Semicolon", "Win+Slash", "Win+Plus", "Win+Minus" })
                Check(KeyChord.Parse(keys).ToShortcut() == keys);
            Check(KeyChord.Parse("Win").Extended && KeyChord.Parse("Win+PrtScn").Key == 0x2c);
            var semicolon = KeyChord.Parse("Win+Semicolon");
            Check(semicolon.ResolveSymbol(0xba).Key == 0xba); // US
            Check(semicolon.ResolveSymbol(0xbb).Key == 0xbb); // JIS
            var plus = KeyChord.Parse("Win+Plus").ResolveSymbol(0x1bb);
            Check(plus.Key == 0xbb && plus.Modifiers.Contains((ushort)0x10) && plus.Modifiers.Contains((ushort)0x5b));
            Throws(() => semicolon.ResolveSymbol(-1));
            Throws(() => semicolon.ResolveSymbol(0));
            Throws(() => semicolon.ResolveSymbol(0x8ba));
            Throws(() => KeyChord.Parse("Ctrl+Win"));
        });
        tests.Add(("static catalog covers both source sections, preserves descriptions and ignores removed custom settings", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            await applet.ActivateAsync(context, default);
            var expected = "Win Win+A Win+Shift+A Win+Alt+B Win+C Win+Alt+D Win+Alt+Down Win+Alt+H Win+Alt+K Win+Alt+Up Win+Ctrl+C Win+Ctrl+Enter Win+Ctrl+F Win+Ctrl+Q Win+Ctrl+Space Win+Ctrl+V Win+D Win+Down Win+E Win+Escape Win+F Win+Slash Win+G Win+H Win+Home Win+I Win+J Win+K Win+Left Win+M Win+Minus Win+N Win+O Win+P Win+Pause Win+Period Win+Semicolon Win+Plus Win+PrintScreen Win+Q Win+R Win+Right Win+S Win+Shift+Down Win+Shift+Enter Win+Shift+Left Win+Shift+M Win+Shift+R Win+Shift+Right Win+Shift+S Win+Shift+Space Win+Shift+Up Win+Shift+V Win+Space Win+Tab Win+U Win+Up Win+V Win+W Win+X Win+Y Win+Z Win+Ctrl+D Win+Ctrl+Right Win+Ctrl+Left Win+Ctrl+F4".Split(' ');
            Check(WindowsShortcuts.All.Select(s => s.Keys).Order().SequenceEqual(expected.Order()));
            Check(WindowsShortcuts.All.Select(s => s.Id).Distinct().Count() == expected.Length);
            foreach (var shortcut in WindowsShortcuts.All) {
                Check(context.Descriptions[context.ExtensionId + "." + shortcut.Suffix] == shortcut.CommandDescription);
                await context.Execute(shortcut.Suffix);
                Check(context.SentKeys[^1] == shortcut.Keys);
            }
            Check(context.SentKeys.Count == expected.Length);
            var fixedIds = context.Handlers.Keys.ToHashSet();
            await context.Change("shortcuts", new[] { new { id = "obsolete", title = "Old", keys = "invalid" } });
            Check(context.Handlers.Count == FixedCount && context.Handlers.Keys.ToHashSet().SetEquals(fixedIds));
            Check(context.Handlers.Keys.All(id => !id.Contains(".send.")));
            var queued = context.Handlers[context.ExtensionId + ".windows.explorer"];
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await ThrowsAsync(() => context.Handlers[context.ExtensionId + ".windows.explorer"](cancellation.Token));
            await applet.DeactivateAsync(default); await queued(default);
            Check(context.SentKeys.Count == expected.Length);
        }));
        tests.Add(("dedicated lock requests one lock without changing automatic lock settings", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            await applet.ActivateAsync(context, default);
            await context.Execute("lock-windows");
            Check(platform.Locks == 1 && context.Values.Count == 0 && context.SentKeys.Count == 0);
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            var queued = context.Handlers[context.ExtensionId + ".lock-windows"];
            await ThrowsAsync(() => queued(cancellation.Token));
            await applet.DeactivateAsync(default); await queued(default); Check(platform.Locks == 1);
        }));
        Test("display waits until deadline, runs once", () => {
            var timer = new DisplayCountdown(); timer.Schedule(100, 5);
            Check(!timer.TakeDue(5099)); Check(timer.TakeDue(5100)); Check(!timer.TakeDue(99999));
        });
        Test("display reschedule and cancel", () => {
            var timer = new DisplayCountdown(); timer.Schedule(0, 5); timer.Schedule(4000, 5);
            Check(!timer.TakeDue(5000)); timer.Cancel(); Check(!timer.TakeDue(99999));
        });
        Test("idle begins when enabled and rearms only after input", () => {
            var policy = new IdleLockPolicy(); policy.Reset(100_000);
            Check(!policy.TakeDue(100_000, 0, 1)); Check(!policy.TakeDue(159_999, 0, 1));
            Check(policy.TakeDue(160_000, 0, 1)); Check(!policy.TakeDue(250_000, 0, 1));
            Check(!policy.TakeDue(250_000, 249_000, 1)); Check(policy.TakeDue(309_000, 249_000, 1));
        });
        Test("idle counter wrap-around", () => {
            var policy = new IdleLockPolicy(); var start = (long)uint.MaxValue - 30_000;
            policy.Reset(start); Check(!policy.TakeDue(start + 59_999, (uint)start, 1));
            Check(policy.TakeDue(start + 60_000, (uint)start, 1));
        });
        tests.Add(("obsolete key settings stay untouched and never create commands", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            var old = new[] { new { id = "new-tab", title = "Old setting", keys = "invalid old key" } };
            context.Values["shortcuts"] = old;
            await applet.ActivateAsync(context, default); Check(context.Handlers.Count == FixedCount);
            await context.Change("displayOffSeconds", 20);
            Check(context.Handlers.Count == FixedCount && context.Values["shortcuts"] == old);
            Check(context.Handlers.Keys.All(id => !id.Contains(".send.")));
            await applet.DeactivateAsync(default); Check(context.Timer is null && context.Changed is null);
        }));
        tests.Add(("display countdown executes, cancels and stops on deactivate", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            await applet.ActivateAsync(context, default); await context.Execute("display-off");
            platform.Uptime = 4999; await context.Tick(); Check(platform.Offs == 0);
            platform.Uptime = 5000; await context.Tick(); Check(platform.Offs == 1);
            await context.Tick(); Check(platform.Offs == 1);
            await context.Execute("display-off"); await context.Execute("cancel-display-off");
            platform.Uptime = 20000; await context.Tick(); Check(platform.Offs == 1);
            await context.Execute("display-off"); var queuedTick = context.Timer!;
            await applet.DeactivateAsync(default); platform.Uptime = 30000;
            await queuedTick(default); Check(platform.Offs == 1);
        }));
        tests.Add(("idle lock opt-in, threshold, input reset, disable", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            await applet.ActivateAsync(context, default);
            platform.Uptime = 100_000_000; await context.Tick(); Check(platform.Locks == 0);
            await context.Change("autoLockMinutes", 1); await context.Execute("toggle-auto-lock");
            await context.Tick(); Check(platform.Locks == 0);
            platform.Uptime += 60_000; await context.Tick(); Check(platform.Locks == 1);
            platform.Uptime += 60_000; await context.Tick(); Check(platform.Locks == 1);
            await context.Execute("toggle-auto-lock"); platform.Input = (uint)platform.Uptime;
            platform.Uptime += 60_000; await context.Tick(); Check(platform.Locks == 1);
            await applet.DeactivateAsync(default);
        }));
        tests.Add(("folder delegates once; canceled command has no effects", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            await applet.ActivateAsync(context, default); await context.Execute("open-folder"); Check(platform.Folders == 1);
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await ThrowsAsync(() => context.Handlers[context.ExtensionId + ".open-folder"](cancellation.Token)); Check(platform.Folders == 1);
            await applet.DeactivateAsync(default);
        }));
        tests.Add(("all window commands survive settings replacement and stop on deactivate", async () => {
            var context = new FakeContext(); var platform = new FakePlatform(); var applet = new WindowsToolsApplet(platform);
            var suffixes = new[] { "left", "right", "full-height", "center", "minimize", "maximize", "restore", "close" };
            var commands = new[] { MoveCommand.Left, MoveCommand.Right, MoveCommand.FullHeight, MoveCommand.Center,
                MoveCommand.Minimize, MoveCommand.Maximize, MoveCommand.Restore, MoveCommand.Close };
            await applet.ActivateAsync(context, default);
            Check(context.LastPanel!.Actions!.Count == 12);
            foreach (var suffix in suffixes) await context.Execute(suffix);
            Check(platform.WindowCommands.SequenceEqual(commands));
            await context.Change("displayOffSeconds", 20);
            foreach (var suffix in suffixes) Check(context.Handlers.ContainsKey(context.ExtensionId + "." + suffix));
            Check(context.Handlers.Count == FixedCount);
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await ThrowsAsync(() => context.Handlers[context.ExtensionId + ".close"](cancellation.Token));
            Check(platform.WindowCommands.Count == 8);
            var queued = context.Handlers[context.ExtensionId + ".close"];
            await applet.DeactivateAsync(default); await queued(default);
            Check(platform.WindowCommands.Count == 8);
        }));
        var failed = 0;
        foreach (var (name, test) in tests) {
            try { await test(); Console.WriteLine("PASS " + name); }
            catch (Exception error) { failed++; Console.Error.WriteLine("FAIL " + name + ": " + error); }
        }
        Console.WriteLine($"{tests.Count - failed}/{tests.Count} passed"); return failed == 0 ? 0 : 1;
    }
    private static void Check(bool value) { if (!value) throw new InvalidOperationException("Assertion failed"); }
    private static void Throws(Action action) { try { action(); } catch (ArgumentException) { return; } throw new InvalidOperationException("Expected rejection"); }
    private static async Task ThrowsAsync(Func<Task> action) { try { await action(); } catch (Exception) { return; } throw new InvalidOperationException("Expected rejection"); }
    private sealed class FakePlatform : IWindowsPlatform
    {
        public long Uptime { get; set; }
        public uint Input;
        public int Offs, Locks, Folders;
        public List<MoveCommand> WindowCommands = [];
        public uint LastInputTimestamp() => Input;
        public void OpenForegroundFolder() => Folders++;
        public void TurnOffDisplay() => Offs++;
        public void Lock() => Locks++;
        public void ExecuteWindow(MoveCommand command, CancellationToken token) { token.ThrowIfCancellationRequested(); WindowCommands.Add(command); }
    }
    private sealed class FakeContext : IExtensionContext, ICommandService, ISettingsService, IUiService, ISchedulerService, IAppDockService
    {
        public string ExtensionId => "at365.windows-tools";
        public Dictionary<string, Func<CancellationToken, Task>> Handlers = [];
        public Dictionary<string, string> Titles = [];
        public Dictionary<string, object> Values = [];
        public Dictionary<string, string?> Descriptions = [];
        public List<string> SentKeys = [];
        public IAppDockService AppDock => this;
        public Task<JsonElement> CallAsync(string method, object? parameters = null, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            Check(method == "input.sendKeys");
            SentKeys.Add(JsonSerializer.SerializeToElement(parameters).GetProperty("keys").GetString()!);
            return Task.FromResult(JsonSerializer.SerializeToElement(new { completion = "inputSubmitted", effectVerified = false }));
        }
        public Func<CancellationToken, Task>? Changed, Timer;
        public AppDock.SDK.Panel? LastPanel;
        public ICommandService Commands => this;
        public ISettingsService Settings => this;
        public IUiService Ui => this;
        public ISchedulerService Scheduler => this;
        public ITrayService Tray => throw new NotSupportedException();
        public INotificationService Notifications => throw new NotSupportedException();
        public IBrowserService Browser => throw new NotSupportedException();
        public ILogService Log => throw new NotSupportedException();
        public IStorageService Storage => throw new NotSupportedException();
        public ISecretService Secrets => throw new NotSupportedException();
        public void Register(string id, string title, Func<CancellationToken, Task> handler) { Handlers.Add(id, handler); Titles.Add(id, title); }
        public Task ReplaceAsync(IReadOnlyList<CommandRegistration> commands, CancellationToken token = default) {
            Handlers = commands.ToDictionary(c => c.Id, c => c.Handler); Titles = commands.ToDictionary(c => c.Id, c => c.Title); Descriptions = commands.ToDictionary(c => c.Id, c => c.Description); return Task.CompletedTask;
        }
        public T Get<T>(string key, T fallback) => Values.TryGetValue(key, out var value) ? JsonSerializer.SerializeToElement(value).Deserialize<T>()! : fallback;
        public Task SetAsync<T>(string key, T value, CancellationToken token = default) { Values[key] = value!; return Task.CompletedTask; }
        public IDisposable OnChanged(Func<CancellationToken, Task> handler) { Changed = handler; return new Release(() => Changed = null); }
        public Task SetOptionsAsync(string key, IReadOnlyList<SettingOption> options, CancellationToken token = default) => throw new NotSupportedException();
        public Task ShowPanelAsync(AppDock.SDK.Panel panel, CancellationToken token = default) { LastPanel = panel; return Task.CompletedTask; }
        public Task<string> GetImageDirectoryAsync(CancellationToken token = default) => throw new NotSupportedException();
        public IDisposable Every(TimeSpan interval, Func<CancellationToken, Task> callback) { Timer = callback; return new Release(() => Timer = null); }
        public Task Execute(string suffix) => Handlers[ExtensionId + "." + suffix](default);
        public Task Tick() => Timer!(default);
        public async Task Change(string key, object value) { Values[key] = value; if (Changed is not null) await Changed(default); }
        private sealed class Release(Action action) : IDisposable { public void Dispose() => action(); }
    }
}
