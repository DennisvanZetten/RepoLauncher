using System.Text.Json;
using RepoLauncher.Models;

namespace RepoLauncher.Services;

public sealed class LauncherSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");

    public async Task<LauncherSettings> LoadAsync()
    {
        if (!File.Exists(_settingsPath))
            return new LauncherSettings();

        try
        {
            await using var stream = File.OpenRead(_settingsPath);
            return await JsonSerializer.DeserializeAsync<LauncherSettings>(stream, JsonOptions)
                ?? new LauncherSettings();
        }
        catch (JsonException)
        {
            return new LauncherSettings();
        }
    }

    public async Task SaveAsync(LauncherSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
    }
}
