// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Client;

public class InvalidApiException : Exception
{
    public InvalidApiException()
    {
    }

    public InvalidApiException(string message)
        : base(message)
    {
    }
}
