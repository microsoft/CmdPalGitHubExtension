// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;

namespace GitHubExtension;

internal sealed partial class SavedQueriesPage : ListPage
{
#pragma warning disable IDE0044 // Add readonly modifier
    private List<QueryPage> _savedQueries;
#pragma warning restore IDE0044 // Add readonly modifier

    public SavedQueriesPage()
    {
        Icon = new IconInfo(string.Empty);
        Name = "Saved Queries";
        _savedQueries = new List<QueryPage>();
        SaveQueryForm.QuerySaved += OnQuerySaved;
    }

    public override IListItem[] GetItems()
    {
        if (_savedQueries.Count > 0)
        {
            var queries = _savedQueries.Select(savedQuery => new ListItem(savedQuery)
            {
                Title = savedQuery.Title,
                Icon = new IconInfo(string.Empty),
            }).ToList();

            queries.Add(new(new SaveQueryPage())
            {
                Title = "Add a query",
                Icon = new IconInfo(string.Empty),
            });
            queries.Add(new(new SaveQueryPage(QueryInput.Survey))
            {
                Title = "Add a query (full form)",
                Icon = new IconInfo(string.Empty),
            });

            return queries.ToArray();
        }
        else
        {
            return new ListItem[]
            {
                new(new SearchIssuesPage())
                {
                    Title = "Sample Query: Search GitHub Issues",
                    Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
                },
                new(new SaveQueryPage(QueryInput.Survey))
                {
                    Title = "Add a query (full form)",
                    Icon = new IconInfo(string.Empty),
                },
                new(new SaveQueryPage())
                {
                    Title = "Add a query by string",
                    Icon = new IconInfo(string.Empty),
                },
            };
        }
    }

    private void OnQuerySaved(object sender, object? args)
    {
        if (args is Exception)
        {
            // do nothing
        }
        else if (args != null && args is Query)
        {
            AddQuery((Query)args);
        }
    }

    private void AddQuery(Query query)
    {
        _savedQueries.Add(new QueryPage(query));
        RaiseItemsChanged(_savedQueries.Count + 1);
    }
}
