using KeyChord = AppDock.SDK.InputKeyChord;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Applets.WindowsTools;

internal sealed record Shortcut(string Id, string Title, KeyChord Chord)
{
    public static IReadOnlyList<Shortcut> Parse(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > 32)
            throw new ArgumentException("送信するショートカットは32件以下の一覧で指定してください。");
        var result = new List<Shortcut>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in value.EnumerateArray())
        {
            string Read(string key) => entry.ValueKind == JsonValueKind.Object && entry.TryGetProperty(key, out var property)
                && property.ValueKind == JsonValueKind.String ? property.GetString()! : "";
            var id = Read("id");
            var title = Read("title");
            if (!Regex.IsMatch(id, "^[a-z0-9][a-z0-9-]{0,39}$") || !ids.Add(id)
                || string.IsNullOrWhiteSpace(title) || title.Length > 100)
                throw new ArgumentException("ショートカットのID・名前・IDの重複を確認してください。");
            result.Add(new(id, title, KeyChord.Parse(Read("keys"))));
        }
        return result;
    }
}
