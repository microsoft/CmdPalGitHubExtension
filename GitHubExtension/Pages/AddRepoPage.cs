// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension.Pages;

internal sealed partial class AddRepoPage : FormPage
{
    private readonly AddRepoForm _addRepoForm;

    public AddRepoPage(SearchIssuesPage searchIssuesPage)
    {
        _addRepoForm = new AddRepoForm();
        _addRepoForm.RepositoryAdded += OnRepositoryAdded;
        _addRepoForm.RepositoryAdded += searchIssuesPage.OnRepositoryAdded; // Subscribe to the event
    }

    public override IForm[] Forms() => new IForm[] { _addRepoForm };

    private void OnRepositoryAdded(object sender, object? args)
    {
        if (args is Exception ex)
        {
            var message = new StatusMessage() { Message = $"Error in adding repository: {ex.Message}", State = MessageState.Error };
            ExtensionHost.Host?.ShowStatus(message);
        }
        else
        {
            var message = new StatusMessage() { Message = "Repository added successfully!", State = MessageState.Success };
            ExtensionHost.Host?.ShowStatus(message);
        }
    }
}
