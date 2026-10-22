namespace RepoLauncher.Models;

public sealed class LauncherSettings
{
    public string PowerShellExecutable { get; set; } = "pwsh.exe";
    public string ToolCommand { get; set; } = "opencode";
    public List<RepositoryEntry> Repositories { get; set; } = [];
}

public sealed class RepositoryEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
