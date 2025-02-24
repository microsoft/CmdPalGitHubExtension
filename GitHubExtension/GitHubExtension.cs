// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using GitHubExtension.Client;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

[ComVisible(true)]
[Guid("a3d8cde8-9dd1-4f2f-85f0-77ea15f3ee8f")]
[ComDefaultInterface(typeof(IExtension))]
public sealed partial class GitHubExtension : IExtension
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly CommandProvider _commandProvider;

    private GitHubClientProvider GitHubClientProvider { get; set; }

    private readonly ILogger _log = Log.ForContext("SourceContext", nameof(GitHubExtension));

    public GitHubExtension(ManualResetEvent extensionDisposedEvent, CommandProvider provider)
    {
        GitHubClientProvider = new GitHubClientProvider();
        _extensionDisposedEvent = extensionDisposedEvent;
        _commandProvider = provider;
    }

    public object GetProvider(ProviderType providerType)
    {
        switch (providerType)
        {
            case ProviderType.Commands:
                return _commandProvider;
            default:
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}
