// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

[Serializable]
public class SaveSearchException : Exception
{
    public SaveSearchException()
    {
    }

    public SaveSearchException(string message)
        : base(message)
    {
    }

    public SaveSearchException(Exception innerException)
        : base("An error occurred while saving the search.", innerException)
    {
    }

    public SaveSearchException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
