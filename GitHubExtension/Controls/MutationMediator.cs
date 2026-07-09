// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.Controls;

// Raised after a write action (close/reopen/merge, etc.) completes so that
// surfaces showing the mutated item can refresh. Uses a weak event source so
// that transient pages that subscribe do not leak.
public class MutationMediator
{
    private readonly WeakEventSource<EventArgs> _mutationCompletedSource = new();

    public event EventHandler<EventArgs>? MutationCompleted
    {
        add => _mutationCompletedSource.Subscribe(value);
        remove => _mutationCompletedSource.Unsubscribe(value);
    }

    public MutationMediator()
    {
    }

    public void NotifyMutationCompleted()
    {
        _mutationCompletedSource.Raise(this, EventArgs.Empty);
    }
}
