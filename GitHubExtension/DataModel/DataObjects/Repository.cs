// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubExtension.DataModel.DataObjects;

public class Repository
{
    public long Id { get; set; } = -1;

    public long InternalId { get; set; } = -1;

    public long OwnerId { get; set; } = -1;

    public string Name { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string CloneUrl { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Private { get; set; }

    public int Fork { get; set; }

    public string DefaultBranch { get; set; } = string.Empty;

    public string Visibility { get; set; } = string.Empty;

    public int HasIssues { get; set; }

    public string FullName { get; private set; } = string.Empty;

    public override string ToString() => FullName;

    // Create repository from OctoKit repo.
    public static Repository CreateFromOctokitRepository(Octokit.Repository octokitRepository)
    {
        var repo = new Repository
        {
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
        };

        // Owner is a rowId in the User table
        var owner = User.CreateFromOctokitUser(octokitRepository.Owner);
        repo.OwnerId = owner.Id;

        return repo;
    }
}
