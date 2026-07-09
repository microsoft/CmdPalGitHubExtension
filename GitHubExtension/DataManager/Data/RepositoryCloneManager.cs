// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class RepositoryCloneManager : IRepositoryCloneManager
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(RepositoryCloneManager));

    private readonly IGitService _gitService;
    private readonly ICloneSettingsStore _settingsStore;

    public RepositoryCloneManager(IGitService gitService, ICloneSettingsStore settingsStore)
    {
        _gitService = gitService;
        _settingsStore = settingsStore;
    }

    public async Task<string> CloneRepositoryAsync(string repository, string? targetDirectory = null, CancellationToken cancellationToken = default)
    {
        if (!_gitService.IsGitInstalled())
        {
            throw new InvalidOperationException("Git is not installed or could not be found on the PATH.");
        }

        var (cloneUrl, repositoryName) = ResolveCloneUrl(repository);

        var baseDirectory = string.IsNullOrWhiteSpace(targetDirectory)
            ? await _settingsStore.GetCloneBaseDirectoryAsync()
            : targetDirectory!.Trim();

        var destination = Path.Combine(baseDirectory, repositoryName);

        if (Directory.Exists(destination) && Directory.EnumerateFileSystemEntries(destination).Any())
        {
            throw new InvalidOperationException($"Destination directory '{destination}' already exists and is not empty.");
        }

        await _gitService.CloneAsync(cloneUrl, destination, cancellationToken);
        _log.Information($"Cloned {cloneUrl} to {destination}.");
        return destination;
    }

    internal static (string CloneUrl, string RepositoryName) ResolveCloneUrl(string repository)
    {
        if (string.IsNullOrWhiteSpace(repository))
        {
            throw new ArgumentException("Repository must be provided.", nameof(repository));
        }

        var trimmed = repository.Trim();

        // A full URL (GitHub HTML URL or a clone URL) is used as-is; the repo
        // name is the last path segment with any trailing ".git" removed.
        if (Validation.IsValidHttpUri(trimmed, out var uri) && uri != null)
        {
            var name = uri.Segments.Length > 0 ? uri.Segments[^1].TrimEnd('/') : string.Empty;
            if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^4];
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Could not determine the repository name from the URL.", nameof(repository));
            }

            return (trimmed, name);
        }

        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo(trimmed);
        return ($"https://github.com/{owner}/{repo}.git", repo);
    }
}
