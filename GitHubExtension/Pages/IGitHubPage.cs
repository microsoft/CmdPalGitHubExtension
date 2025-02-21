﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal partial interface IGitHubPage : IPage
{
    public void OnLoadingStateChanged(object sender, bool isLoading);

    public void ToastStatusMessage();

    public void SetStatusMessage(string message, MessageState state);
}
