using System.ComponentModel;
using System.Diagnostics;

namespace RepoLauncher.Services;

public sealed class GitBranchService
{
    public async Task<GitBranchInfo> GetCurrentBranchAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return new GitBranchInfo(null, "Pad bestaat niet");

        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(path);
        startInfo.ArgumentList.Add("rev-parse");
        startInfo.ArgumentList.Add("--abbrev-ref");
        startInfo.ArgumentList.Add("HEAD");

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
                return new GitBranchInfo(null, "Git kon niet worden gestart");

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var output = (await outputTask).Trim();
            var error = await errorTask;

            if (process.ExitCode == 0)
            {
                return output == "HEAD"
                    ? new GitBranchInfo(null, "Detached HEAD")
                    : new GitBranchInfo(output, null);
            }

            return error.Contains("not a git repository", StringComparison.OrdinalIgnoreCase)
                ? new GitBranchInfo(null, "Geen Git-repository")
                : new GitBranchInfo(null, "Branch niet beschikbaar");
        }
        catch (Win32Exception)
        {
            return new GitBranchInfo(null, "Git is niet beschikbaar");
        }
        catch (InvalidOperationException)
        {
            return new GitBranchInfo(null, "Branch niet beschikbaar");
        }
    }
}

public sealed record GitBranchInfo(string? Branch, string? Status);
