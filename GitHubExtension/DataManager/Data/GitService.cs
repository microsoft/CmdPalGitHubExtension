// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Serilog;

namespace GitHubExtension.DataManager.Data;

// Default IGitService backed by the local git executable. Git availability is
// determined by locating git on the PATH, and cloning shells out to
// "git clone <url> <destination>". Under MSIX the process launch and the target
// directory are subject to the app sandbox; failures surface as exceptions that
// the calling form/command turns into an error toast.
public sealed class GitService : IGitService
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(GitService));

    public bool IsGitInstalled() => FindGitExecutable() != null;

    public async Task CloneAsync(string cloneUrl, string destinationPath, CancellationToken cancellationToken = default)
    {
        var gitExecutable = FindGitExecutable()
            ?? throw new InvalidOperationException("Git is not installed or could not be found on the PATH.");

        var parentDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = gitExecutable,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("clone");
        startInfo.ArgumentList.Add(cloneUrl);
        startInfo.ArgumentList.Add(destinationPath);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _log.Error($"git clone failed with exit code {process.ExitCode}: {standardError}");
            throw new InvalidOperationException($"git clone failed (exit code {process.ExitCode}): {standardError}");
        }

        _log.Information($"Cloned {cloneUrl} into {destinationPath}.");
    }

    private static string? FindGitExecutable()
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return null;
        }

        foreach (var directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var candidate = Path.Combine(directory, "git.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch (ArgumentException)
            {
                // Ignore malformed PATH entries.
            }
        }

        return null;
    }
}
