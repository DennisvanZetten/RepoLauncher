using System.Diagnostics;
using RepoLauncher.Models;

namespace RepoLauncher.Services;

public sealed class PowerShellLauncher
{
    public int LaunchAll(LauncherSettings settings, IReadOnlyCollection<RepositoryEntry> repositories)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "wt.exe",
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("-w");
        startInfo.ArgumentList.Add("new");

        var startedCount = 0;
        foreach (var repository in repositories)
        {
            if (!Directory.Exists(repository.Path))
                continue;

            if (startedCount > 0)
                startInfo.ArgumentList.Add(";");

            startInfo.ArgumentList.Add("new-tab");
            startInfo.ArgumentList.Add("--title");
            startInfo.ArgumentList.Add(string.IsNullOrWhiteSpace(repository.Name)
                ? Path.GetFileName(Path.TrimEndingDirectorySeparator(repository.Path))
                : repository.Name);
            startInfo.ArgumentList.Add("-d");
            startInfo.ArgumentList.Add(repository.Path);
            startInfo.ArgumentList.Add(settings.PowerShellExecutable);
            startInfo.ArgumentList.Add("-NoExit");
            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add(settings.ToolCommand);
            startedCount++;
        }

        if (startedCount == 0)
            return 0;

        if (Process.Start(startInfo) is null)
            throw new InvalidOperationException("Windows Terminal kon niet worden gestart.");

        return startedCount;
    }
}
