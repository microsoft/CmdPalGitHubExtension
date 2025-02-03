// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension.Pages;

internal sealed partial class TestPage : FormPage
{
    private readonly TestForm _testForm;

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _testStatusMessage;
#pragma warning restore IDE0044 // Add readonly modifier

    public TestPage()
    {
        _testForm = new();
        _testForm.RepositoryAdded += OnRepositoryAdded;
        _testForm.LoadingStateChanged += OnLoadingChanged;
        _testStatusMessage = new StatusMessage();

        // Call OnPageOpened when the page is first loaded
        OnPageOpened();
    }

    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_testStatusMessage);
        return new IForm[] { _testForm };
    }

    private void OnPageOpened()
    {
        // Your custom logic for when the page is opened
        _testStatusMessage.Message = "Page opened successfully!";
        _testStatusMessage.State = MessageState.Info;
        ExtensionHost.ShowStatus(_testStatusMessage);
    }

    private void OnRepositoryAdded(object sender, object? args)
    {
        IsLoading = false;
        if (args is Exception ex)
        {
            _testStatusMessage.Message = $"Error in adding repository: {ex.Message}";
            _testStatusMessage.State = MessageState.Error;
        }
        else
        {
            _testStatusMessage.Message = "Repository added successfully!";
            _testStatusMessage.State = MessageState.Success;
        }

        ExtensionHost.ShowStatus(_testStatusMessage);
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
