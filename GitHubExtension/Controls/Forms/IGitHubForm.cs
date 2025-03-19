// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Windows.Foundation;

namespace GitHubExtension.Controls.Forms;

public interface IGitHubForm
{
    event EventHandler<bool>? LoadingStateChanged;

    event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    Dictionary<string, string> TemplateSubstitutions { get; }
}
