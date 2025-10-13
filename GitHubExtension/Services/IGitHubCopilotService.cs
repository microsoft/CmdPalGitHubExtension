// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.DataObjects;

namespace GitHubExtension.Services;

public interface IGitHubCopilotService
{
    Task<string> SendMessageAsync(string message);

    bool IsAvailable();

    Task<IEnumerable<CopilotTask>> GetCopilotTasksAsync();
}
