// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Commands;

// Clones a repository from a list item into the configured clone base directory,
// showing a success or error toast. The clone runs synchronously within Invoke
// so the toast reflects the final outcome.
internal sealed partial class CloneRepositoryCommand : InvokableCommand
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(CloneRepositoryCommand));

    private readonly IRepository _repository;
    private readonly IRepositoryCloneManager _cloneManager;
    private readonly IResources _resources;

    internal CloneRepositoryCommand(IRepository repository, IRepositoryCloneManager cloneManager, IResources resources)
    {
        _repository = repository;
        _cloneManager = cloneManager;
        _resources = resources;
        Name = resources.GetResource("Commands_CloneRepository");
        Icon = new IconInfo("\uE896");
    }

    public override CommandResult Invoke()
    {
        try
        {
            var cloneSource = string.IsNullOrWhiteSpace(_repository.CloneUrl) ? _repository.FullName : _repository.CloneUrl;
            _cloneManager.CloneRepositoryAsync(cloneSource).GetAwaiter().GetResult();
            ToastHelper.ShowSuccessToast(_resources.GetResource("Message_CloneRepository_Success"));
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to clone repository.");
            ToastHelper.ShowErrorToast(_resources.GetResource("Message_CloneRepository_Error"));
        }

        return CommandResult.KeepOpen();
    }
}
