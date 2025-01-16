// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Windows.Security.Credentials;

namespace GitHubExtension.DeveloperId;

internal interface ICredentialVault
{
    PasswordCredential? GetCredentials(string loginId);

    void RemoveCredentials(string loginId);

    void SaveCredentials(string loginId, SecureString? accessToken);

    IEnumerable<string> GetAllCredentials();

    void RemoveAllCredentials();
}