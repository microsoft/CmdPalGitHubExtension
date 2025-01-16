// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Client;

public class InvalidGitHubUrlException : Exception
{
    public InvalidGitHubUrlException()
    {
    }

    public InvalidGitHubUrlException(string message)
        : base(message)
    {
    }
}
