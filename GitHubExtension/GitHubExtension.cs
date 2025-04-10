// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

[Guid("a3d8cde8-9dd1-4f2f-85f0-77ea15f3ee8f")]
public sealed partial class GitHubExtension : IExtension
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    public event EventHandler<ManualResetEvent>? Release;

    private readonly CommandProvider _commandProvider;

    private readonly ILogger _log = Log.ForContext("SourceContext", nameof(GitHubExtension));

#pragma warning disable IDE0290 // Use primary constructor
    public GitHubExtension(ManualResetEvent extensionDisposedEvent, CommandProvider commandProvider)
#pragma warning restore IDE0290 // Use primary constructor
    {
        _extensionDisposedEvent = extensionDisposedEvent;
        _commandProvider = commandProvider;
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

    public void Dispose() => Release?.Invoke(this, _extensionDisposedEvent);
}
