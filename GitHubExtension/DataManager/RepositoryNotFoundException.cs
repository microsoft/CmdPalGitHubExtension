// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

public class RepositoryNotFoundException : ApplicationException
{
    public RepositoryNotFoundException()
    {
    }

    public RepositoryNotFoundException(string message)
        : base(message)
    {
    }
}
