// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using GitHubExtension.Client;
using Microsoft.CommandPalette.Extensions;
using Serilog;

namespace GitHubExtension;

[ComVisible(true)]
[Guid("a3d8cde8-9dd1-4f2f-85f0-77ea15f3ee8f")]
[ComDefaultInterface(typeof(IExtension))]
public sealed partial class GitHubExtension : IExtension
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private GitHubClientProvider GitHubClientProvider { get; set; }

    private readonly ILogger _log = Log.ForContext("SourceContext", nameof(GitHubExtension));

#pragma warning disable IDE0290 // Use primary constructor
    public GitHubExtension(ManualResetEvent extensionDisposedEvent)
#pragma warning restore IDE0290 // Use primary constructor
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
        this.GitHubClientProvider = new GitHubClientProvider();
    }

    public object? GetProvider(ProviderType providerType)
    {
        switch (providerType)
        {
            case ProviderType.Commands:
                return new GitHubExtensionCommandsProvider();
            default:
                return null;
        }
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}
