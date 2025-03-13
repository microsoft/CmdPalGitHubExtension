// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Controls.Forms;

public partial class SignInForm : FormContent, IGitHubForm
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignInAction;

    public event TypedEventHandler<object, bool>? LoadingStateChanged;

    public event TypedEventHandler<object, FormSubmitEventArgs>? FormSubmitted;

    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;

    public SignInForm(IDeveloperIdProvider developerIdProvider, IResources resources)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AuthTitle}}", _resources.GetResource("Forms_Sign_In") },
        { "{{AuthButtonTitle}}", _resources.GetResource("Forms_Sign_In") },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon("logo")}" },
        { "{{AuthButtonTooltip}}", _resources.GetResource("Forms_Sign_In_Tooltip") },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("AuthTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        LoadingStateChanged?.Invoke(this, true);
        Task.Run(() =>
        {
            try
            {
                var signInSucceeded = HandleSignIn().Result;
                LoadingStateChanged?.Invoke(this, false);
                SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(signInSucceeded, null));
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(signInSucceeded, null));
            }
            catch (Exception ex)
            {
                LoadingStateChanged?.Invoke(this, false);
                SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(false, ex));
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
            }
        });
        return CommandResult.KeepOpen();
    }

    private async Task<bool> HandleSignIn()
    {
        var numPreviousDevIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Count();

        await _developerIdProvider.LoginNewDeveloperIdAsync();

        var numDevIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Count();

        return numDevIds > numPreviousDevIds;
    }
}
