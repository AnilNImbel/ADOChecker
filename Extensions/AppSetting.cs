
using System.Text.Json;

public static class AppSettingsReader
{
    private static readonly JsonElement _root;

    static AppSettingsReader()
    {
        var json = File.ReadAllText("appsettings.json");
        _root = JsonDocument.Parse(json).RootElement;
    }

    public static string GetValue(string section, string key)
    {
        return _root.GetProperty(section).GetProperty(key).GetString();
    }
}
