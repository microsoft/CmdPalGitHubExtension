// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;
using Serilog;

namespace GitHubExtension.Controls.Forms;

public partial class GitHubMcpForm : FormContent, IGitHubForm, IDisposable
{
    private static readonly Lazy<ILogger> _logger = new(() => Log.ForContext("SourceContext", nameof(GitHubMcpForm)));
    private static readonly ILogger _log = _logger.Value;

    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;
    private bool _isProcessing;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public GitHubMcpForm(IDeveloperIdProvider developerIdProvider, IResources resources)
    {
        _developerIdProvider = developerIdProvider;
        _resources = resources;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{McpIcon}}", JsonSerializer.Serialize($"data:image/png;base64,{GitHubIcon.GetBase64Icon(GitHubIcon.LogoWithBackplatePath)}") },
        { "{{McpTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_Title")) },
        { "{{McpDescription}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_Description")) },
        { "{{RepositoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_RepositoryLabel")) },
        { "{{RepositoryPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_RepositoryPlaceholder")) },
        { "{{RepositoryErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_RepositoryError")) },
        { "{{BaseBranchLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_BaseBranchLabel")) },
        { "{{BaseBranchPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_BaseBranchPlaceholder")) },
        { "{{TitleLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_TitleLabel")) },
        { "{{TitlePlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_TitlePlaceholder")) },
        { "{{TitleErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_TitleError")) },
        { "{{DescriptionLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_DescriptionLabel")) },
        { "{{DescriptionPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_DescriptionPlaceholder")) },
        { "{{SubmitButtonTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubMcp_SubmitButton")) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("GitHubMcpTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        _log.Information("=== SubmitForm called ===");
        _log.Information($"Inputs: {inputs}");
        _log.Information($"Data: {data}");
        _log.Information($"Is processing: {_isProcessing}");

        if (_isProcessing)
        {
            _log.Information("Form is already processing, ignoring duplicate submission");
            ToastHelper.ShowErrorToast("Task creation already in progress...");
            return CommandResult.KeepOpen();
        }

        try
        {
            var inputData = JsonSerializer.Deserialize<Dictionary<string, object>>(inputs);
            if (inputData == null)
            {
                _log.Error("Failed to deserialize input data");
                ToastHelper.ShowErrorToast("Failed to parse form data");
                return CommandResult.KeepOpen();
            }

            var repository = inputData?.TryGetValue("Repository", out var repoObj) == true ? repoObj.ToString()?.Trim() ?? string.Empty : string.Empty;
            var baseBranch = inputData?.TryGetValue("BaseBranch", out var branchObj) == true ? branchObj.ToString()?.Trim() : string.Empty;

            // Default to "main" if baseBranch is empty
            if (string.IsNullOrEmpty(baseBranch))
            {
                baseBranch = "main";
            }

            var title = inputData?.TryGetValue("Title", out var titleObj) == true ? titleObj.ToString()?.Trim() ?? string.Empty : string.Empty;
            var description = inputData?.TryGetValue("Description", out var descObj) == true ? descObj.ToString()?.Trim() ?? string.Empty : string.Empty;

            _log.Information($"Parsed form data:");
            _log.Information($"  Repository: '{repository}'");
            _log.Information($"  Branch: '{baseBranch}'");
            _log.Information($"  Title: '{title}'");
            _log.Information($"  Description length: {description.Length}");

            if (string.IsNullOrEmpty(repository) || string.IsNullOrEmpty(title))
            {
                _log.Warning("Repository or Title is empty, cannot submit");
                var error = new InvalidOperationException("Repository and Title are required");
                LoadingStateChanged?.Invoke(this, false);
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, error));
                ToastHelper.ShowErrorToast("Repository and Title are required");
                return CommandResult.KeepOpen();
            }

            // Validate repository format
            if (!repository.Contains('/'))
            {
                _log.Warning($"Invalid repository format: {repository}");
                var error = new InvalidOperationException("Repository must be in the format 'owner/repository'");
                LoadingStateChanged?.Invoke(this, false);
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, error));
                ToastHelper.ShowErrorToast("Repository must be in the format 'owner/repository'");
                return CommandResult.KeepOpen();
            }

            // Set processing flag immediately
            _isProcessing = true;
            _log.Information("Starting async task creation...");

            // Show immediate feedback
            ToastHelper.ShowToast("Creating Copilot Workspace task...", MessageState.Success);

            // Start the async operation with proper continuation
            _ = Task.Run(async () =>
            {
                try
                {
                    _log.Information("Task.Run started");
                    await CreateCopilotTaskAsync(repository, baseBranch, title, description);
                    _log.Information("Task.Run completed successfully");
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Unhandled error in Task.Run");
                    _isProcessing = false;
                    LoadingStateChanged?.Invoke(this, false);
                    FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
                }
            });

            _log.Information("SubmitForm returning");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error in SubmitForm");
            _isProcessing = false;
            LoadingStateChanged?.Invoke(this, false);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
            ToastHelper.ShowErrorToast($"Error: {ex.Message}");
        }

        return CommandResult.KeepOpen();
    }

    private async Task CreateCopilotTaskAsync(string repository, string baseBranch, string title, string description)
    {
        _log.Information($"=== CreateCopilotTaskAsync START ===");
        _log.Information($"Repository: {repository}");
        _log.Information($"BaseBranch: {baseBranch}");
        _log.Information($"Title: {title}");
        _log.Information($"Description length: {description?.Length ?? 0}");

        LoadingStateChanged?.Invoke(this, true);

        try
        {
            _log.Information("Step 1: Getting developer IDs...");

            // Get the developer ID and GitHub client
            var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();

            if (devIds == null)
            {
                _log.Error("devIds is null!");
                throw new InvalidOperationException("No developer IDs available. Please sign in to GitHub.");
            }

            var devIdsList = devIds.ToList();
            _log.Information($"Found {devIdsList.Count} developer ID(s)");

            if (devIdsList.Count == 0)
            {
                _log.Error("No developer IDs found in the list!");
                throw new InvalidOperationException("You are not signed in to GitHub. Please sign in first.");
            }

            var devId = devIdsList.FirstOrDefault();

            if (devId == null)
            {
                _log.Error("devId is null after FirstOrDefault!");
                throw new InvalidOperationException("No developer ID available. Please sign in to GitHub.");
            }

            _log.Information($"Step 2: Got developer ID: {devId.LoginId}");
            _log.Information($"Developer ID URL: {devId.Url}");

            if (devId.GitHubClient == null)
            {
                _log.Error("GitHubClient is null!");
                throw new InvalidOperationException("GitHub client is not initialized. Please sign out and sign in again.");
            }

            var client = devId.GitHubClient;
            _log.Information($"Step 3: Got GitHub client successfully");

            // Verify client authentication
            try
            {
                var currentUser = await client.User.Current();
                _log.Information($"Step 4: Verified authentication - Current user: {currentUser.Login}");
            }
            catch (Exception authEx)
            {
                _log.Error(authEx, "Failed to verify GitHub authentication");
                throw new InvalidOperationException($"Failed to authenticate with GitHub: {authEx.Message}. Please sign out and sign in again.");
            }

            // Parse the repository (owner/repo format)
            _log.Information($"Step 5: Parsing repository: {repository}");
            var repoParts = repository.Split('/');
            if (repoParts.Length != 2)
            {
                _log.Error($"Invalid repository format: {repository} (parts count: {repoParts.Length})");
                throw new InvalidOperationException("Repository must be in the format 'owner/repository'");
            }

            var owner = repoParts[0].Trim();
            var repoName = repoParts[1].Trim();

            _log.Information($"Step 6: Parsed repository - Owner: '{owner}', Repo: '{repoName}'");

            // Verify repository exists and user has access
            try
            {
                _log.Information($"Step 7: Verifying repository access...");
                var repo = await client.Repository.Get(owner, repoName);
                _log.Information($"Step 8: Repository verified - ID: {repo.Id}, Full Name: {repo.FullName}");
            }
            catch (Octokit.NotFoundException)
            {
                _log.Error($"Repository {owner}/{repoName} not found or no access");
                throw new InvalidOperationException($"Repository '{owner}/{repoName}' not found or you don't have access to it. Please check the repository name and your permissions.");
            }
            catch (Exception repoEx)
            {
                _log.Error(repoEx, $"Error verifying repository access");
                throw new InvalidOperationException($"Error accessing repository: {repoEx.Message}");
            }

            _log.Information($"Step 9: Building issue body...");

            // Create an issue with the special label that triggers Copilot Workspace
            var issueBody = BuildTaskDescription(description ?? string.Empty, baseBranch);
            _log.Information($"Step 10: Issue body created (length: {issueBody.Length})");

            var newIssue = new NewIssue(title)
            {
                Body = issueBody,
            };

            // Add the copilot-workspace label to trigger the agent
            newIssue.Labels.Add("copilot-workspace");
            _log.Information($"Step 11: NewIssue object created with title: '{title}' and label: 'copilot-workspace'");

            _log.Information($"Step 12: Calling GitHub API to create issue in {owner}/{repoName}...");

            // Create the issue
            Octokit.Issue createdIssue;
            try
            {
                createdIssue = await client.Issue.Create(owner, repoName, newIssue);
                _log.Information($"Step 13: ? SUCCESS - Issue created!");
                _log.Information($"  Issue Number: #{createdIssue.Number}");
                _log.Information($"  Issue ID: {createdIssue.Id}");
                _log.Information($"  Issue URL: {createdIssue.HtmlUrl}");
                _log.Information($"  Issue State: {createdIssue.State}");
            }
            catch (Octokit.ApiException apiEx)
            {
                _log.Error(apiEx, $"GitHub API error creating issue");
                _log.Error($"  Status Code: {apiEx.StatusCode}");
                _log.Error($"  Message: {apiEx.Message}");
                if (apiEx.HttpResponse != null)
                {
                    _log.Error($"  Response Body: {apiEx.HttpResponse.Body}");
                }

                throw new InvalidOperationException($"Failed to create issue: {apiEx.Message}");
            }
            catch (Exception issueEx)
            {
                _log.Error(issueEx, $"Unexpected error creating issue");
                throw;
            }

            // Construct the Copilot Workspace URL
            var copilotWorkspaceUrl = $"https://copilot-workspace.githubnext.com/{owner}/{repoName}/issues/{createdIssue.Number}?baseBranch={Uri.EscapeDataString(baseBranch)}";

            _log.Information($"Step 14: Copilot Workspace URL constructed:");
            _log.Information($"  {copilotWorkspaceUrl}");
            _log.Information($"Step 15: Opening browser...");

            // Open the browser to the Copilot Workspace task
            try
            {
                var browserOpened = OpenBrowser(copilotWorkspaceUrl);

                if (browserOpened)
                {
                    _log.Information("Step 16: ? Browser opened successfully!");
                }
                else
                {
                    _log.Warning("Step 16: ? Browser may not have opened successfully");

                    // Try alternative method: copy URL to clipboard and show message
                    ToastHelper.ShowToast($"Browser may not have opened. Issue URL: {createdIssue.HtmlUrl}", MessageState.Warning);
                }
            }
            catch (Exception browserEx)
            {
                _log.Error(browserEx, "Failed to open browser");

                // Show the URL in a toast so user can manually navigate
                ToastHelper.ShowToast($"Please open: {copilotWorkspaceUrl}", MessageState.Warning);
            }

            _log.Information("Step 17: Invoking success callbacks...");
            LoadingStateChanged?.Invoke(this, false);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(true, null));

            // Show success toast
            ToastHelper.ShowSuccessToast($"Task created successfully! Issue #{createdIssue.Number}");

            _log.Information($"=== CreateCopilotTaskAsync COMPLETED SUCCESSFULLY ===");
        }
        catch (Octokit.NotFoundException ex)
        {
            _log.Error(ex, $"Repository not found or no access: {repository}");
            LoadingStateChanged?.Invoke(this, false);
            var error = new InvalidOperationException($"Repository not found or you don't have access to it: {ex.Message}");
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, error));
            ToastHelper.ShowErrorToast($"Repository not found: {repository}");
        }
        catch (Octokit.ApiException ex)
        {
            _log.Error(ex, $"GitHub API error: {ex.StatusCode} - {ex.Message}");
            LoadingStateChanged?.Invoke(this, false);
            var error = new InvalidOperationException($"GitHub API error: {ex.Message}");
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, error));
            ToastHelper.ShowErrorToast($"GitHub error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _log.Error(ex, "Invalid operation");
            LoadingStateChanged?.Invoke(this, false);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
            ToastHelper.ShowErrorToast(ex.Message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Unexpected error creating Copilot Workspace task");
            LoadingStateChanged?.Invoke(this, false);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
            ToastHelper.ShowErrorToast($"Error: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            _log.Information($"=== CreateCopilotTaskAsync END (finally block) ===");
        }
    }

    private static string BuildTaskDescription(string description, string baseBranch)
    {
        var body = string.Empty;

        if (!string.IsNullOrEmpty(description))
        {
            body = description;
        }

        if (!string.IsNullOrEmpty(baseBranch))
        {
            if (!string.IsNullOrEmpty(body))
            {
                body += "\n\n";
            }

            body += $"Base Branch: {baseBranch}";
        }

        return body;
    }

    private static bool OpenBrowser(string url)
    {
        try
        {
            _log.Information($"OpenBrowser called with URL: {url}");

            // Validate URL
            if (string.IsNullOrWhiteSpace(url))
            {
                _log.Error("URL is null or empty");
                return false;
            }

            // Try to parse as URI to validate
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _log.Error($"Invalid URL format: {url}");
                return false;
            }

            // Use ProcessStartInfo to open the default browser
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            };

            _log.Information("ProcessStartInfo created, calling Process.Start...");
            var process = Process.Start(psi);

            if (process != null)
            {
                _log.Information($"Process started successfully. Process ID: {process.Id}");
                return true;
            }

            _log.Warning("Process.Start returned null");

            // Try alternative method using cmd /c start
            try
            {
                _log.Information("Trying alternative method with cmd /c start...");
                var cmdPsi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" \"{url}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var cmdProcess = Process.Start(cmdPsi);
                if (cmdProcess != null)
                {
                    _log.Information("Alternative method succeeded");
                    return true;
                }
            }
            catch (Exception altEx)
            {
                _log.Error(altEx, "Alternative method also failed");
            }

            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to open browser to {url}");
            _log.Error($"Exception type: {ex.GetType().FullName}");
            _log.Error($"Exception message: {ex.Message}");
            if (ex.InnerException != null)
            {
                _log.Error($"Inner exception: {ex.InnerException.Message}");
            }

            return false;
        }
    }

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose any managed resources if needed
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
