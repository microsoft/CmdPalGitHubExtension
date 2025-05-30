﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Foundation;

namespace GitHubExtension.DeveloperIds;

public interface IDeveloperIdProvider
{
    IEnumerable<IDeveloperId> GetLoggedInDeveloperIdsInternal();

    IDeveloperId? GetLoggedInDeveloperId();

    IDeveloperId GetDeveloperIdInternal(IDeveloperId devId);

    IAsyncOperation<IDeveloperId> LoginNewDeveloperIdAsync();

    bool LogoutDeveloperId(IDeveloperId developerId);

    void HandleOauthRedirection(Uri authorizationResponse);

    bool IsSignedIn();

    event EventHandler<Exception?>? OAuthRedirected;
}
