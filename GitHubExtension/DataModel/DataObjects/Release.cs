// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("Release")]
public class Release
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Release)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    // Repository table
    public long RepositoryId { get; set; } = DataStore.NoForeignKey;

    public string Name { get; set; } = string.Empty;

    public string TagName { get; set; } = string.Empty;

    public long Prerelease { get; set; } = DataStore.NoForeignKey;

    public string HtmlUrl { get; set; } = string.Empty;

    public long TimeCreated { get; set; } = DataStore.NoForeignKey;

    public long TimePublished { get; set; } = DataStore.NoForeignKey;

    public long TimeLastObserved { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    private DataStore? DataStore
    {
        get; set;
    }

    [Write(false)]
    [Computed]
    public DateTime CreatedAt => TimeCreated.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime? PublishedAt => TimePublished != 0 ? TimePublished.ToDateTime() : null;

    [Write(false)]
    [Computed]
    public DateTime LastObservedAt => TimeLastObserved.ToDateTime();

    public override string ToString() => Name;

    public static Release GetOrCreateByOctokitRelease(DataStore dataStore, Octokit.Release okitRelease, Repository repository)
    {
        var release = CreateFromOctokitRelease(dataStore, okitRelease, repository);
        return AddOrUpdateRelease(dataStore, release);
    }

    public static IEnumerable<Release> GetAllForRepository(DataStore dataStore, Repository repository)
    {
        var sql = $"SELECT * FROM Release WHERE RepositoryId = @RepositoryId ORDER BY TimePublished DESC;";
        var param = new
        {
            RepositoryId = repository.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var releases = dataStore.Connection!.Query<Release>(sql, param, null) ?? Enumerable.Empty<Release>();
        foreach (var release in releases)
        {
            release.DataStore = dataStore;
        }

        return releases;
    }

    public static Release? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = $"SELECT * FROM Release WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        var release = dataStore.Connection!.QueryFirstOrDefault<Release>(sql, param, null);
        if (release is not null)
        {
            // Add Datastore so this object can make internal queries.
            release.DataStore = dataStore;
        }

        return release;
    }

    public static void DeleteLastObservedBefore(DataStore dataStore, long repositoryId, DateTime date)
    {
        // Delete releases older than the time specified for the given repository.
        // This is intended to be run after updating a repository's releases so that non-observed
        // records will be removed.
        var sql = @"DELETE FROM Release WHERE RepositoryId = $RepositoryId AND TimeLastObserved < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$RepositoryId", repositoryId);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    private static Release CreateFromOctokitRelease(DataStore dataStore, Octokit.Release okitRelease, Repository repository)
    {
        var release = new Release
        {
            DataStore = dataStore,
            InternalId = okitRelease.Id,
            RepositoryId = repository.Id,
            Name = okitRelease.Name,
            TagName = okitRelease.TagName,
            Prerelease = okitRelease.Prerelease ? 1 : 0,
            HtmlUrl = okitRelease.HtmlUrl,
            TimeCreated = okitRelease.CreatedAt.DateTime.ToDataStoreInteger(),
            TimePublished = okitRelease.PublishedAt.HasValue ? okitRelease.PublishedAt.Value.DateTime.ToDataStoreInteger() : 0,
            TimeLastObserved = DateTime.UtcNow.ToDataStoreInteger(),
        };

        return release;
    }

    private static Release AddOrUpdateRelease(DataStore dataStore, Release release)
    {
        // Check for existing release data.
        var existing = GetByInternalId(dataStore, release.InternalId);
        if (existing is not null)
        {
            // Existing releases must be updated and always marked observed.
            release.Id = existing.Id;
            dataStore.Connection!.Update(release);
            release.DataStore = dataStore;
            return release;
        }

        // No existing release, add it.
        release.Id = dataStore.Connection!.Insert(release);
        release.DataStore = dataStore;
        return release;
    }

    public static void DeleteBefore(DataStore dataStore, DateTime date)
    {
        // Delete releases older than the date listed.
        var sql = @"DELETE FROM Release WHERE TimeLastObserved < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
