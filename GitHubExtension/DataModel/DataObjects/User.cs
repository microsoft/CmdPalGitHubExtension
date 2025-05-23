// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

[Table("User")]
public class User
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(User)}"));

    private static readonly ILogger _log = _logger.Value;

    // This is the time between seeing a potential updated user record and updating it.
    // This value / 2 is the average time between user updating their user data and having
    // it reflected in the datastore.
    private static readonly long _updateThreshold = TimeSpan.FromHours(4).Ticks;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public string Login { get; set; } = string.Empty;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    public string AvatarUrl { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    private DataStore? DataStore { get; set; }

    [Write(false)]
    [Computed]
    public IEnumerable<PullRequest> PullRequests
    {
        get
        {
            if (DataStore == null)
            {
                return Enumerable.Empty<PullRequest>();
            }
            else
            {
                return PullRequest.GetAllForUser(DataStore, this) ?? Enumerable.Empty<PullRequest>();
            }
        }
    }

    public override string ToString() => Login;

    private static User CreateFromOctokitUser(Octokit.User user)
    {
        return new User
        {
            InternalId = user.Id,
            Login = user.Login,
            AvatarUrl = user.AvatarUrl ?? string.Empty,
            Type = user.Type.HasValue ? user.Type.Value.ToString() : string.Empty,
            TimeUpdated = DateTime.UtcNow.ToDataStoreInteger(),
        };
    }

    public static User AddOrUpdateUser(DataStore dataStore, User user)
    {
        // Check for existing user data.
        var existingUser = GetByInternalId(dataStore, user.InternalId);
        if (existingUser is not null)
        {
            // Many of the same user records will be created on a sync, and to
            // avoid unnecessary updating and database operations for data that
            // is extremely unlikely to have changed in any significant way, we
            // will only update every UpdateThreshold amount of time.
            if ((user.TimeUpdated - existingUser.TimeUpdated) > _updateThreshold)
            {
                user.Id = existingUser.Id;
                dataStore.Connection!.Update(user);
                user.DataStore = dataStore;
                return user;
            }
            else
            {
                return existingUser;
            }
        }

        // No existing pull request, add it.
        user.Id = dataStore.Connection!.Insert(user);
        user.DataStore = dataStore;
        return user;
    }

    public static User? GetById(DataStore dataStore, long id)
    {
        var user = dataStore.Connection!.Get<User>(id);
        if (user != null)
        {
            user.DataStore = dataStore;
        }

        return user;
    }

    public static User? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = @"SELECT * FROM User WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        var user = dataStore.Connection!.QueryFirstOrDefault<User>(sql, param, null);
        if (user != null)
        {
            user.DataStore = dataStore;
        }

        return user;
    }

    public static User GetOrCreateByOctokitUser(DataStore dataStore, Octokit.User user)
    {
        var newUser = CreateFromOctokitUser(user);
        return AddOrUpdateUser(dataStore, newUser);
    }
}
