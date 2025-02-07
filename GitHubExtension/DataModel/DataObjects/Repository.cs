// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("Repository")]
public class Repository
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Repository)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    // User table
    public long OwnerId { get; set; } = DataStore.NoForeignKey;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    public long Private { get; set; } = DataStore.NoForeignKey;

    public string HtmlUrl { get; set; } = string.Empty;

    public string CloneUrl { get; set; } = string.Empty;

    public long Fork { get; set; } = DataStore.NoForeignKey;

    public string DefaultBranch { get; set; } = string.Empty;

    public string Visibility { get; set; } = string.Empty;

    public long HasIssues { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    public long TimePushed { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    private DataStore? DataStore
    {
        get; set;
    }

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime PushedAt => TimePushed.ToDateTime();

    [Write(false)]
    [Computed]
    public string FullName => Owner.Login + '/' + Name;

    [Write(false)]
    [Computed]
    public User Owner
    {
        get
        {
            if (DataStore == null)
            {
                return new User();
            }
            else
            {
                return User.GetById(DataStore, OwnerId) ?? new User();
            }
        }
    }

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
                return PullRequest.GetAllForRepository(DataStore, this) ?? Enumerable.Empty<PullRequest>();
            }
        }
    }

    [Write(false)]
    [Computed]
    public IEnumerable<Issue> Issues
    {
        get
        {
            if (DataStore == null)
            {
                return Enumerable.Empty<Issue>();
            }
            else
            {
                return Issue.GetAllForRepository(DataStore, this) ?? Enumerable.Empty<Issue>();
            }
        }
    }

    [Write(false)]
    [Computed]
    public IEnumerable<Release> Releases
    {
        get
        {
            if (DataStore == null)
            {
                return Enumerable.Empty<Release>();
            }
            else
            {
                return Release.GetAllForRepository(DataStore, this) ?? Enumerable.Empty<Release>();
            }
        }
    }

    public IEnumerable<Issue> GetIssuesForQuery(string query)
    {
        if (DataStore == null)
        {
            return Enumerable.Empty<Issue>();
        }
        else
        {
            var search = Search.Get(DataStore, query, Id);
            if (search is null)
            {
                return Enumerable.Empty<Issue>();
            }

            return Issue.GetForSearch(DataStore, search);
        }
    }

    public override string ToString() => FullName;

    // Create repository from OctoKit repo.
    private static Repository CreateFromOctokitRepository(DataStore dataStore, Octokit.Repository octokitRepository)
    {
        var repo = new Repository
        {
            DataStore = dataStore,
            Name = octokitRepository.Name,                                  // Cannot be null.
            HtmlUrl = octokitRepository.HtmlUrl ?? string.Empty,
            CloneUrl = octokitRepository.CloneUrl ?? string.Empty,
            Description = octokitRepository.Description ?? string.Empty,
            InternalId = octokitRepository.Id,                              // Cannot be null.
            Private = octokitRepository.Private ? 1 : 0,
            Fork = octokitRepository.Fork ? 1 : 0,
            DefaultBranch = octokitRepository.DefaultBranch ?? string.Empty,
            Visibility = octokitRepository.Visibility.HasValue ? octokitRepository.Visibility.Value.ToString() : string.Empty,
            HasIssues = octokitRepository.HasIssues ? 1 : 0,
            TimeUpdated = octokitRepository.UpdatedAt.DateTime.ToDataStoreInteger(),
            TimePushed = octokitRepository.UpdatedAt.DateTime.ToDataStoreInteger(),
        };

        // Owner is a rowId in the User table
        var owner = User.GetOrCreateByOctokitUser(dataStore, octokitRepository.Owner);
        repo.OwnerId = owner.Id;

        return repo;
    }

    private static Repository AddOrUpdateRepository(DataStore dataStore, Repository repository)
    {
        // Check for existing repository data.
        var existingRepository = GetByInternalId(dataStore, repository.InternalId);
        if (existingRepository is not null)
        {
            // Only update if the TimeUpdated is different. That is our clue that something changed.
            // We will see a lot of repository AddOrUpdate when inserting pull requests and issues, so
            // avoid unnecessary work or database operations.
            if (existingRepository.TimeUpdated < repository.TimeUpdated)
            {
                repository.Id = existingRepository.Id;
                dataStore.Connection!.Update(repository);
                repository.DataStore = dataStore;
                return repository;
            }
            else
            {
                return existingRepository;
            }
        }

        // No existing repository, add it.
        repository.Id = dataStore.Connection!.Insert(repository);
        repository.DataStore = dataStore;
        return repository;
    }

    public static Repository? GetById(DataStore dataStore, long id)
    {
        var repo = dataStore.Connection!.Get<Repository>(id);
        if (repo is not null)
        {
            // Add Datastore so this object can make internal queries.
            repo.DataStore = dataStore;
        }

        return repo;
    }

    public static Repository? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = @"SELECT * FROM Repository WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        var repo = dataStore.Connection!.QueryFirstOrDefault<Repository>(sql, param, null);
        if (repo is not null)
        {
            // Add Datastore so this object can make internal queries.
            repo.DataStore = dataStore;
        }

        return repo;
    }

    public static Repository GetOrCreateByOctokitRepository(DataStore dataStore, Octokit.Repository octokitRepository)
    {
        var repository = CreateFromOctokitRepository(dataStore, octokitRepository);
        return AddOrUpdateRepository(dataStore, repository);
    }

    public static IEnumerable<Repository> GetAll(DataStore dataStore)
    {
        var repositories = dataStore.Connection!.GetAll<Repository>() ?? Enumerable.Empty<Repository>();
        foreach (var repository in repositories)
        {
            repository.DataStore = dataStore;
        }

        return repositories;
    }

    public static Repository? Get(DataStore dataStore, string owner, string name)
    {
        var sql = @"SELECT * FROM Repository AS R WHERE R.Name = @Name AND R.OwnerId IN (SELECT Id FROM User WHERE User.Login = @Owner)";
        var param = new
        {
            Name = name,
            Owner = owner,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var repo = dataStore.Connection!.QueryFirstOrDefault<Repository>(sql, param, null);
        if (repo is not null)
        {
            repo.DataStore = dataStore;
        }

        return repo;
    }

    public static Repository? Get(DataStore dataStore, string fullName)
    {
        var nameSplit = fullName.Split(['/'], 2);
        if (nameSplit.Length != 2)
        {
            _log.Warning($"Invalid fullName input into Repository.Get: {fullName}");
            return null;
        }

        return Get(dataStore, nameSplit[0], nameSplit[1]);
    }
}
