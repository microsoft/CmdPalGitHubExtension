// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Pages;

namespace GitHubExtension.DataManager;

internal sealed class PullRequestSourceBranchDecorator : IPullRequest
{
    private readonly IPullRequestUpdater _pullRequestUpdater;
    private IPullRequest _source;

    public PullRequestSourceBranchDecorator(IPullRequest source, IPullRequestUpdater dataManager)
    {
        _source = source;
        _pullRequestUpdater = dataManager;
    }

    public string Title => _source.Title;

    public long Number => _source.Number;

    public string SourceBranch
    {
        get
        {
            if (string.IsNullOrEmpty(_source.SourceBranch))
            {
                // We use an awaiter here so we don't expose it as async to the consumer.
                // We may want to revisit this if we need to do more work here.
                // The issue is that it might create a delay in the UI.
                _source = _pullRequestUpdater.UpdatePullRequestFromPullRequestAPIAsync(_source).GetAwaiter().GetResult();
            }

            return _source.SourceBranch;
        }
    }

    public string Body => _source.Body;

    public string HtmlUrl => _source.HtmlUrl;

    public IEnumerable<ILabel> Labels => _source.Labels;
}
