# Fix for Private Repository Access

## Problem
The logs show:
```
User has 12 public repos, 0 private repos
```

But you actually have 10 private repos that should be accessible.

## Root Cause

Your current GitHub OAuth token **does not have the `repo` scope** granted. Even though the application _requests_ this scope during authentication, you need to re-authenticate for GitHub to prompt you to grant these permissions.

## Verification

The OAuth scopes requested by the application are defined in `GitHubExtension/DeveloperId/OAuthRequest.cs`:

```csharp
var request = new OauthLoginRequest(OauthConfiguration.GetClientId())
{
    Scopes = { "read:user", "notifications", "repo", "read:org", "write:org" },
    State = State,
    RedirectUri = new Uri(OauthConfiguration.RedirectUri),
};
```

The `"repo"` scope **is included**, which grants:
- Full access to private repositories
- Ability to read and write code
- Access to private repo metadata

## Solution: Re-authenticate

You need to sign out and sign back in to grant the application the `repo` scope:

### Steps:

1. **Open Command Palette** (Win + Alt + Space)

2. **Sign Out**
   - Search for "github"
   - Select "Sign out of the GitHub Extension (Preview)"
   - Confirm sign out

3. **Sign In Again**
   - Search for "github"
   - Select "Sign in to the GitHub Extension (Preview)"
   - Click "Sign in"

4. **Authorize on GitHub**
   - Your browser will open to GitHub's OAuth authorization page
   - **Important**: You'll see a list of permissions the app is requesting
   - Look for: **"Access to private repositories"** or **"repo" scope**
   - Click "Authorize" to grant these permissions

5. **Verify in Logs**
   - After signing in, go to "GitHub Copilot" ? "View Task"
   - Check the logs at: `%localappdata%\Packages\Microsoft.CmdPalGitHubExtension_*\TempState\GitHubExtension-*.dhlog`
   - Look for the new diagnostic section:

```
OAuth Scopes Granted:
  - read:user
  - notifications
  - repo ?
  - read:org
  - write:org

? 'repo' scope is granted (private repos should be accessible)
User has 12 public repos, 10 private repos
```

## What to Look For in Logs

### ? Success - Scopes Granted
```
OAuth Scopes Granted:
  - repo
? 'repo' scope is granted (private repos should be accessible)
User has 12 public repos, 10 private repos
```

### ?? Problem - Missing Scope
```
OAuth Scopes Granted:
  - read:user
  - notifications
??  WARNING: 'repo' scope is NOT granted!
This is why you cannot see private repositories.
Solution: Sign out and sign in again to grant the 'repo' scope.
```

## Why This Happens

- **First-time authorization**: If you signed in before the `repo` scope was added to the code, your token wouldn't have it
- **Cached credentials**: Windows Credential Manager stores the old token without the new permissions
- **OAuth behavior**: GitHub OAuth tokens don't automatically gain new scopes - you must re-authorize

## Changes Made

I've added OAuth scope diagnostics to `GitHubCopilotService.cs` that will:
1. Display all OAuth scopes granted to your token
2. Specifically check for the `repo` scope
3. Warn you if it's missing with clear instructions
4. Show the correct private repo count after re-authentication

## Testing

After re-authenticating, the logs should show:
- ? All OAuth scopes including `repo`
- ? Correct private repo count: `User has 12 public repos, 10 private repos`
- ? Ability to access private repos in searches
- ? Private PRs showing up in Copilot tasks

## Troubleshooting

If you still don't see private repos after re-authenticating:

1. **Check GitHub OAuth App Settings**
   - Visit: https://github.com/settings/applications
   - Find "Command Palette GitHub Extension"
   - Click on it
   - Verify it has "repo" access listed

2. **Revoke and Re-authorize**
   - On the same page, click "Revoke"
   - Sign in to the extension again
   - This forces a fresh authorization

3. **Check Organization Settings**
   - If your private repos are in an organization:
   - Visit: https://github.com/settings/connections/applications/[app-id]
   - Look for "Organization access"
   - Click "Grant" or "Request" for organizations with private repos

## Additional Notes

- The `repo` scope grants **full access** to private repositories (read/write)
- If you only need read access, GitHub doesn't offer a read-only private repo scope
- This is a one-time step - once granted, the scope persists until you revoke it

