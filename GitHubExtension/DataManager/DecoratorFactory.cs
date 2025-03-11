// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

public class DecoratorFactory : IDecoratorFactory
{
    private readonly IPullRequestUpdater _pullRequestUpdater;

    public DecoratorFactory(IPullRequestUpdater pullRequestUpdater)
    {
        _pullRequestUpdater = pullRequestUpdater;
    }

    public IPullRequest DecorateSearchBranch(IPullRequest pullRequest)
    {
        return new PullRequestSourceBranchDecorator(pullRequest, _pullRequestUpdater);
    }
}

public interface IDecoratorFactory
{
    IPullRequest DecorateSearchBranch(IPullRequest pullRequest);
}
