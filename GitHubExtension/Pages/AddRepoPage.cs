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

    public AddRepoPage()
    {
        _addRepoForm = new();
        _addRepoForm.RepositoryAdded += OnRepositoryAdded;
        _addRepoForm.LoadingStateChanged += OnLoadingChanged;
    }

    public override IForm[] Forms() => new IForm[] { _addRepoForm };

    private void OnRepositoryAdded(object sender, object? args)
    {
        IsLoading = false;
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

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
