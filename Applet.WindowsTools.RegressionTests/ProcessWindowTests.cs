using System.Text.Json;
using Applets.WindowsTools;

internal static class ProcessWindowTests
{
    public static void AddTo(List<(string, Func<Task>)> tests)
    {
        void Test(string name, Action action) => tests.Add((name, () => { action(); return Task.CompletedTask; }));
        Test("process name accepts optional exe but rejects paths, wildcards and missing names", () => {
            Require(ProcessWindows.NormalizeName(" mstsc.EXE ") == "mstsc");
            Require(ProcessWindows.NormalizeName("my.app") == "my.app");
            foreach (var name in new[] { "", " ", ".exe", "C:\\mstsc.exe", "a/b", "*", "a?", "a\n.exe", ".." })
                Reject(() => ProcessWindows.NormalizeName(name));
        });
        Test("process config rejects wrong types, unknown actions and extra parameters", () => {
            foreach (var config in new object[] { new { processName = "mstsc" }, new { processName = 1, action = "toggle" },
                new { processName = "mstsc", action = "close" }, new { processName = "mstsc", action = "restore", extra = 1 } })
                Reject(() => ProcessWindows.Parse(JsonSerializer.SerializeToElement(config)));
        });
        Test("toggle handles mixed process states independently and ignores expired windows", () => {
            var system = new FakeSystem(); system.Minimized.Add(2); system.Expired.Add(3);
            new ProcessWindows(system).Execute("example.exe", ProcessWindowAction.Toggle, default);
            Require(system.Name == "example" && system.Requests.SequenceEqual(new[] {
                ((nint)1, ProcessWindowAction.Minimize), ((nint)2, ProcessWindowAction.Restore) }));
        });
        Test("explicit minimize and restore apply to every current main window", () => {
            foreach (var action in new[] { ProcessWindowAction.Minimize, ProcessWindowAction.Restore }) {
                var system = new FakeSystem(); system.Minimized.Add(2);
                new ProcessWindows(system).Execute("example", action, default);
                Require(system.Requests.Count == 3 && system.Requests.All(r => r.Action == action));
            }
        });
        Test("no matching process has no effect or foreground fallback", () => {
            var system = new FakeSystem { Targets = [] };
            new ProcessWindows(system).Execute("missing", ProcessWindowAction.Toggle, default);
            Require(system.Requests.Count == 0);
        });
        Test("cancellation before lookup and between requests stops further effects", () => {
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            var system = new FakeSystem();
            try { new ProcessWindows(system).Execute("example", ProcessWindowAction.Toggle, cancelled.Token); throw new Exception("Expected cancellation"); }
            catch (OperationCanceledException) { Require(system.Name is null && system.Requests.Count == 0); }
            using var midway = new CancellationTokenSource(); system.AfterRequest = midway.Cancel;
            try { new ProcessWindows(system).Execute("example", ProcessWindowAction.Toggle, midway.Token); throw new Exception("Expected cancellation"); }
            catch (OperationCanceledException) { Require(system.Requests.Count == 1); }
        });
        Test("target is rechecked after reading minimized state", () => {
            var system = new FakeSystem { ExpireOnStateRead = true };
            new ProcessWindows(system).Execute("example", ProcessWindowAction.Toggle, default);
            Require(system.Requests.Count == 0);
        });
    }
    private static void Require(bool condition) { if (!condition) throw new Exception("Process-window assertion failed"); }
    private static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } throw new Exception("Expected invalid parameters"); }
    private sealed class FakeSystem : IProcessWindowSystem
    {
        public List<ProcessWindowTarget> Targets = [new(1, 11), new(2, 12), new(3, 13)];
        public HashSet<nint> Minimized = [], Expired = [];
        public List<(nint Handle, ProcessWindowAction Action)> Requests = [];
        public string? Name;
        public Action? AfterRequest;
        public bool ExpireOnStateRead;
        public IReadOnlyList<ProcessWindowTarget> Find(string name) { Name = name; return Targets; }
        public bool IsCurrent(ProcessWindowTarget target) => !Expired.Contains(target.Handle);
        public bool IsMinimized(nint window) { if (ExpireOnStateRead) Expired.Add(window); return Minimized.Contains(window); }
        public void Request(nint window, ProcessWindowAction action) { Requests.Add((window, action)); AfterRequest?.Invoke(); }
    }
}
