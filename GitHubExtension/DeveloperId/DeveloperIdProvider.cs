// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Security;
using Octokit;
using Serilog;
using Windows.Foundation;

namespace GitHubExtension.DeveloperId;

public class DeveloperIdProvider : IDeveloperIdProvider
{
    private static readonly Lock _oAuthRequestsLock = new();

    // DeveloperIdProvider uses singleton pattern.
    // private static readonly Lazy<DeveloperIdProvider> _singletonDeveloperIdProvider = new(() => new DeveloperIdProvider());
    // public static DeveloperIdProvider GetInstance() => _singletonDeveloperIdProvider.Value;
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(DeveloperIdProvider)));

    private static readonly ILogger _log = _logger.Value;

    // Locks to control access to Singleton class members.
    private static readonly Lock _developerIdsLock = new();

    // List of currently active Oauth Request sessions.
    private List<OAuthRequest> OAuthRequests
    {
        get; set;
    }

    // DeveloperId list containing all Logged in Ids.
    private List<DeveloperId> DeveloperIds
    {
        get; set;
    }

    private readonly Lazy<CredentialVault> _credentialVault;

    public event EventHandler<Exception?>? OAuthRedirected;

    // Private constructor for Singleton class.
    public DeveloperIdProvider()
    {
        _credentialVault = new(() => new CredentialVault());

        lock (_oAuthRequestsLock)
        {
            OAuthRequests ??= [];
        }

        lock (_developerIdsLock)
        {
            DeveloperIds ??= [];
        }

        try
        {
            // Retrieve and populate Logged in DeveloperIds from previous launch.
            RestoreDeveloperIds(_credentialVault.Value.GetAllCredentials());
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Error while restoring DeveloperIds: {ex.Message}. Proceeding without restoring.");
        }
    }

    public IAsyncOperation<IDeveloperId> LoginNewDeveloperIdAsync()
    {
        return Task.Run(() =>
        {
            var oauthRequest = LoginNewDeveloperId();
            if (oauthRequest is null)
            {
                _log.Error($"Invalid OAuthRequest");
                throw new InvalidOperationException();
            }

            oauthRequest.AwaitCompletion();

            var devId = CreateOrUpdateDeveloperIdFromOauthRequest(oauthRequest);
            oauthRequest.Dispose();

            _log.Information($"New DeveloperId logged in");

            return devId as IDeveloperId;
        }).AsAsyncOperation();
    }

    private OAuthRequest? LoginNewDeveloperId()
    {
        OAuthRequest oauthRequest = new();

        lock (_oAuthRequestsLock)
        {
            OAuthRequests.Add(oauthRequest);
            try
            {
                oauthRequest.BeginOAuthRequest();
                return oauthRequest;
            }
            catch (Exception ex)
            {
                OAuthRequests.Remove(oauthRequest);
                _log.Error(ex, $"Unable to complete OAuth request: ");
            }
        }

        return null;
    }

    private DeveloperId CreateOrUpdateDeveloperIdFromOauthRequest(OAuthRequest oauthRequest)
    {
        // Query necessary data and populate Developer Id.
        var newDeveloperId = oauthRequest.RetrieveDeveloperId();
        var accessToken = oauthRequest.AccessToken;
        if (accessToken is null)
        {
            _log.Error($"Invalid AccessToken");
            throw new InvalidOperationException();
        }

        _log.Information($"{newDeveloperId.LoginId} logged in with OAuth flow to {newDeveloperId.GetHostAddress()}");

        SaveOrOverwriteDeveloperId(newDeveloperId, accessToken);

        return newDeveloperId;
    }

    public bool LogoutDeveloperId(IDeveloperId developerId)
    {
        DeveloperId? developerIdToLogout;
        lock (_developerIdsLock)
        {
            developerIdToLogout = DeveloperIds?.Find(e => e.LoginId == developerId.LoginId);
            if (developerIdToLogout == null)
            {
                _log.Error($"Unable to find DeveloperId to logout");
                return false;
            }

            _credentialVault.Value.RemoveCredentials(developerIdToLogout.Url);
            DeveloperIds?.Remove(developerIdToLogout);
        }

        return true;
    }

    public void HandleOauthRedirection(Uri authorizationResponse)
    {
        OAuthRequest? oAuthRequest = null;

        OAuthRedirected?.Invoke(this, null);

        lock (_oAuthRequestsLock)
        {
            if (OAuthRequests is null)
            {
                throw new InvalidOperationException();
            }

            if (OAuthRequests.Count is 0)
            {
                // This could happen if the user refreshes the redirected browser window
                // causing the OAuth response to be received again.
                _log.Warning($"No saved OAuth requests to match OAuth response");
                throw new InvalidOperationException($"No saved OAuth requests to match OAuth response. Do not refresh the browser when signing in.");
            }

            var state = OAuthRequest.RetrieveState(authorizationResponse);

            oAuthRequest = OAuthRequests.Find(r => r.State == state);

            if (oAuthRequest == null)
            {
                // This could happen if the user refreshes a previously redirected browser window instead of using
                // the new browser window for the response. Log the warning and return.
                _log.Warning($"Unable to find valid request for received OAuth response");
                return;
            }
            else
            {
                OAuthRequests.Remove(oAuthRequest);
            }
        }

        try
        {
           oAuthRequest.CompleteOAuthAsync(authorizationResponse).Wait();
        }
        catch (Exception ex)
        {
           _log.Error(ex, $"Error while completing OAuth request: ");
           OAuthRedirected?.Invoke(this, ex);
        }
    }

    public IEnumerable<IDeveloperId> GetLoggedInDeveloperIdsInternal()
    {
        List<DeveloperId> iDeveloperIds = [];
        lock (_developerIdsLock)
        {
            iDeveloperIds.AddRange(DeveloperIds);
        }

        return iDeveloperIds;
    }

    // Convert devID to internal devID.
    public IDeveloperId GetDeveloperIdInternal(IDeveloperId devId)
    {
        var devIds = GetLoggedInDeveloperIdsInternal();
        var devIdInternal = devIds.Where(i => i.LoginId.Equals(devId.LoginId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

        return devIdInternal ?? throw new ArgumentException(devId.LoginId);
    }

    private void RestoreDeveloperIds(IEnumerable<string> loginIdsAndUrls)
    {
        foreach (var loginIdOrUrl in loginIdsAndUrls)
        {
            // Since GitHub loginIds cannot contain /, and URLs would, this is sufficient to differentiate between
            // loginIds and URLs. We could alternatively use TryCreate, but there could be some GHES urls that we miss.
            var isUrl = loginIdOrUrl.Contains('/');

            // For loginIds without URL, use GitHub.com as default.
            var hostAddress = isUrl ? new Uri(loginIdOrUrl) : new Uri(Constants.GITHUB_COM_URL);

            GitHubClient gitHubClient = new(new ProductHeaderValue(Constants.CMDPAL_APPLICATION_NAME), hostAddress)
            {
                Credentials = new(_credentialVault.Value.GetCredentials(loginIdOrUrl)?.Password),
            };

            try
            {
                var user = gitHubClient.User.Current().Result;
                DeveloperId developerId = new(user.Login, user.Name, user.Email, user.Url, gitHubClient);
                lock (_developerIdsLock)
                {
                    DeveloperIds.Add(developerId);
                }

                _log.Information($"Restored DeveloperId {user.Url}");

                // If loginId is currently used to save credential, remove it, and use URL instead.
                if (!isUrl)
                {
                    ReplaceSavedLoginIdWithUrl(developerId);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Error while restoring DeveloperId {loginIdOrUrl} : ");

                // If we are unable to restore a DeveloperId, remove it from CredentialManager to avoid
                // the same error next time, and to force the user to login again
                _credentialVault.Value.RemoveCredentials(loginIdOrUrl);
            }
        }

        return;
    }

    private void ReplaceSavedLoginIdWithUrl(DeveloperId developerId)
    {
        try
        {
            _credentialVault.Value.SaveCredentials(
                developerId.Url,
                new NetworkCredential(string.Empty, _credentialVault.Value.GetCredentials(developerId.LoginId)?.Password).SecurePassword);
            _credentialVault.Value.RemoveCredentials(developerId.LoginId);
            _log.Information($"Replaced {developerId.LoginId} with {developerId.Url} in CredentialManager");
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Error while replacing {developerId.LoginId} with {developerId.Url} in CredentialManager: ");
        }
    }

    // Internal Functions.
    private void SaveOrOverwriteDeveloperId(DeveloperId newDeveloperId, SecureString accessToken)
    {
        var duplicateDeveloperIds = DeveloperIds.Where(d => d.Url.Equals(newDeveloperId.Url, StringComparison.OrdinalIgnoreCase));

        if (duplicateDeveloperIds.Any())
        {
            _log.Information($"DeveloperID already exists! Updating accessToken");
            try
            {
                // Save the credential to Credential Vault.
                _credentialVault.Value.SaveCredentials(duplicateDeveloperIds.Single().Url, accessToken);
            }
            catch (InvalidOperationException)
            {
                _log.Warning($"Multiple copies of same DeveloperID already exists");
                throw new InvalidOperationException("Multiple copies of same DeveloperID already exists");
            }
        }
        else
        {
            lock (_developerIdsLock)
            {
                DeveloperIds.Add(newDeveloperId);
            }

            _credentialVault.Value.SaveCredentials(newDeveloperId.Url, accessToken);
        }
    }
}
