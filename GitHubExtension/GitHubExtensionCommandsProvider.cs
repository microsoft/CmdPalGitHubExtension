// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension;

public partial class GitHubExtensionActionsProvider : CommandProvider
{
    public GitHubExtensionActionsProvider()
    {
        DisplayName = "GitHub Extension";
        _authPage = new GitHubAuthPage();

        _authPage.SignInAction += AddSignedInCommands;
    }

    private void AddSignedInCommands(object sender, object? args)
    {
        RaiseItemsChanged(0);
    }

    private static readonly IListItem[] _signedInCommands = [
        new ListItem(new SearchIssuesPage())
        {
            Title = "Search GitHub Issues",
            Icon = new(GitHubIcon.IconDictionary["issue"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
        new ListItem(new SearchPullRequestsPage())
        {
            Title = "Search GitHub Pull Requests",
            Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
        new ListItem(new SearchIssuesPage())
        {
            Title = "Search assigned to me",
            Icon = new(GitHubIcon.IconDictionary["issue"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
        new ListItem(new SearchPullRequestsPage())
        {
            Title = "Search review requested",
            Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
        new ListItem(new SearchMentionsPage())
        {
            Title = "Search mentioned me",
            Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
        new ListItem(new SearchReleasesPage())
        {
            Title = "Search GitHub Releases",
            Icon = new(GitHubIcon.IconDictionary["release"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
        new ListItem(new SearchRepositoriesPage())
        {
            Title = "Search GitHub Repositories",
            Icon = new(GitHubIcon.IconDictionary["logo"]),
            Tags = [new Tag()
            {
                Text = "Command",
            }
            ],
        },
    ];

    private readonly GitHubAuthPage _authPage;

    public override ICommandItem[] TopLevelCommands()
    {
        if (IsSignedIn())
        {
            return _signedInCommands;
        }
        else
        {
            return new[]
            {
                new CommandItem(_authPage)
                    {
                        Title = "GitHub Extension",
                        Subtitle = "Log in.",
                    },
            };
        }
    }

    private static bool IsSignedIn()
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();
        return devIds.Any();
    }
}
