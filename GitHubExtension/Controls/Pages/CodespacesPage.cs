// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;

namespace GitHubExtension.Controls.Pages;

public sealed class CodespacesPage : ListPage
{
    private readonly GitHubClientProvider _gitHubClientProvider;
    private readonly IResources _resources;

    public CodespacesPage(IResources resources, GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
        _resources = resources;
        Icon = GitHubIcon.IconDictionary["logo"];
        Title = resources.GetResource("Pages_Codespaces");
        Name = Title; // Title is for the Page, Name is for the Command
        this.PlaceholderText = resources.GetResource("Pages_Codespaces_Placeholder");
        this.IsLoading = true;
    }

    public override IListItem[] GetItems()
    {
        List<IListItem> result = [];
        try
        {
            CodespacesCollection codespaces = _gitHubClientProvider.GetClientForLoggedInDeveloper().Result.Codespaces.GetAll().Result;

            foreach (Codespace c in codespaces.Codespaces)
            {
                List<IContextItem> moreCommands = [];

                if (IsExecutableInPath("code-insiders"))
                {
                    moreCommands.Add(new CommandContextItem(new OpenUrlCommand("vscode-insiders://github.codespaces/connect?name=" + Uri.EscapeDataString(c.Name) + "&windowId=_blank") { Name = _resources.GetResource("Commands_Open_VS_Code_Insiders"), Icon = IconHelpers.FromRelativePath("Assets\\vscode-insiders.svg") }));
                }

                if (IsExecutableInPath("code"))
                {
                    moreCommands.Add(new CommandContextItem(new OpenUrlCommand("vscode://github.codespaces/connect?name=" + Uri.EscapeDataString(c.Name) + "&windowId=_blank") { Name = _resources.GetResource("Commands_Open_VS_Code"), Icon = IconHelpers.FromRelativePath("Assets\\vscode.svg") }));
                }

                result.Add(new ListItem(new OpenUrlCommand(c.WebUrl))
                {
                    Title = $"{c.Repository.Owner.Login}/{c.Repository.Name}",
                    Subtitle = $"{c.Name} - {c.Machine.DisplayName} ({c.State.StringValue})",
                    Icon = GitHubIcon.IconDictionary["logo"],
                    MoreCommands = [.. moreCommands],
                });
            }
        }
        catch (Exception)
        {
            this.IsLoading = false;
            return [new ListItem(new NoOpCommand()) { Title = _resources.GetResource("Message_Codespaces_LoadFail"), Subtitle = _resources.GetResource("Message_Codespaces_LoadFail_Description") }];
        }

        this.IsLoading = false;
        return [.. result];
    }

    private bool IsExecutableInPath(string searchPath)
    {
        var values = Environment.GetEnvironmentVariable("PATH");
        if (values is null)
        {
            return false;
        }

        foreach (var path in values.Split(Path.PathSeparator))
        {
            var fullPath = Path.Combine(path, searchPath);
            if (File.Exists(fullPath))
            {
                return true;
            }
        }

        return false;
    }
}
