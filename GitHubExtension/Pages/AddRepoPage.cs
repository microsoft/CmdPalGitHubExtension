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

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _addRepoStatusMessage;
#pragma warning restore IDE0044 // Add readonly modifier

    public AddRepoPage()
    {
        _addRepoForm = new();
        _addRepoForm.RepositoryAdded += OnRepositoryAdded;
        _addRepoForm.LoadingStateChanged += OnLoadingChanged;
        _addRepoStatusMessage = new StatusMessage();
    }

    public override IForm[] Forms() => new IForm[] { _addRepoForm };

    private void OnRepositoryAdded(object sender, object? args)
    {
        IsLoading = false;
        if (args is Exception ex)
        {
            _addRepoStatusMessage.Message = $"Error in adding repository: {ex.Message}";
            _addRepoStatusMessage.State = MessageState.Error;
        }
        else
        {
            _addRepoStatusMessage.Message = "Repository added successfully!";
            _addRepoStatusMessage.State = MessageState.Success;
        }

        ExtensionHost.ShowStatus(_addRepoStatusMessage);
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
