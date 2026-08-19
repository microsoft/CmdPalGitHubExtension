---
author: Michael Jolley
created on: 2026-08-19
last updated: 2026-08-19
status: proposal
---

# GitHub Extension 1.0

The GitHub Extension started as a quick way to run saved GitHub queries from Command Palette. That works, but the open issues tell a pretty clear story. People want to do more than search. They want to keep up with their work, move through repositories, and handle common GitHub tasks without bouncing between windows all day.

This spec defines the 1.0 version of the extension as a broader GitHub client built with native Command Palette surfaces.

The goal is not to squeeze all of GitHub into a command palette. Some work needs more room. Reviewing a code diff, changing repository settings, or publishing a release still belongs on GitHub. The extension should make the everyday path faster and hand you off to GitHub when the browser is the better tool.

## Product promise

GitHub Extension 1.0 gives you one place to:

* Keep up with notifications and work that needs your attention.
* Find and open repositories you use often.
* Work with issues, pull requests, Actions, Codespaces, releases, and discussions.
* Start and track Copilot agent tasks through an explicitly labeled Preview experience.
* Keep using the saved queries and top level shortcuts that already work for you.

The extension supports GitHub.com and GitHub Enterprise Cloud with data residency on `ghe.com`. It supports multiple signed in accounts, but one account is active at a time.

## Design principles

### Keep common work close

The extension should complete small, frequent tasks without opening a browser. If a task needs a full editor, a code diff, repository administration, or a destructive action, the extension opens the right GitHub page instead.

### Make side effects obvious

Selecting an item or showing its details must not change GitHub state. Commands that write data say what they will do. Actions with cost, broad scope, or meaningful state changes require confirmation.

### Fail one surface at a time

A problem loading Actions should not take down issues, notifications, or the rest of the extension. Each feature reports its own permission, network, rate limit, and availability errors with a useful next step.

### Preserve what users already built

The 1.0 upgrade migrates saved queries and pin state. Users sign in again only when the new host authentication flow or additional permissions require it. API caches can be discarded and rebuilt.

## Information architecture

### Existing top level commands

The current top level commands remain available:

* Saved GitHub queries
* Assigned to me
* Review requested
* Mentions me
* Created issues
* My pull requests

Users can continue to pin saved queries to the top level.

### GitHub hub

1.0 adds a consolidated GitHub command. It opens the hub with these global destinations:

* Notifications
* Repositories
* Copilot Agents (Preview)
* Codespaces

The hub opens Notifications by default.

### Repository page

Opening a repository provides these destinations:

* Issues
* Pull Requests
* Actions
* Discussions

Repository file browsing is not part of 1.0.

## Feature requirements

### Notifications

The Notifications page shows notifications for the active account, including the repository, subject, reason, unread state, and last update time.

![Example notifications tab](https://github.com/user-attachments/assets/ca85fa69-8a74-44cb-b933-59038de87b69)

Users can:

* Open the related issue, pull request, discussion, workflow, or GitHub page.
* Mark one notification as read.
* Mark the current filtered view as read after confirmation.
* Filter notifications locally by repository, reason, type, and read state.

Opening a notification marks it as read. Selecting it or previewing its details does not.

Polling follows GitHub's `Last-Modified` and `X-Poll-Interval` response headers. A manual refresh cannot bypass the server supplied polling interval.

### Repositories

The Repositories page shows a useful personal list instead of downloading every accessible repository on startup.

The default list contains:

* Pinned repositories.
* Recently opened repositories.
* Recently pushed repositories already known to the local cache.

![Repositories tab example](https://github.com/user-attachments/assets/1243f996-354d-42fe-b581-24132c35e781)

Search runs remotely across repositories available to the active account. Private repositories appear when the account and GitHub application have access.

Users can:

* Open a repository page in Command Palette.
* Open the repository on GitHub.
* Pin or unpin the repository.
* Copy its URL or clone URL.

### Issues

The Issues page supports repository issues and saved issue searches.

![Example issues tab](https://github.com/user-attachments/assets/f293c217-8ca0-49e0-9ab8-d768f2eb847b)

Users can:

* Search, filter, and open issues.
* Read issue content and comments.
* Create an issue with a title and body.
* Create an issue from a supported repository template.
* Add a comment.
* Assign an issue to themselves.
* Assign an issue to Copilot when the Preview agent capability is available.
* Open the issue on GitHub.

Simple Markdown templates and templates that map cleanly to Command Palette forms are rendered in the extension. Unsupported YAML issue forms open on GitHub. The extension does not guess at controls it cannot represent safely.

### Pull requests

The Pull Requests page focuses on status and coordination, not code review.

![Example pull request tab](https://github.com/user-attachments/assets/289d47c5-0303-4b8b-afec-298822d6fccc)

Users can:

* Search, filter, and open pull requests.
* Read the title, description, comments, labels, reviewers, branches, merge state, and check status.
* Add a regular pull request comment.
* Copy the pull request URL, number, source branch, or a supported checkout command.
* Open the pull request or its checks on GitHub.

The extension does not render code diffs, submit reviews, approve, request changes, merge, close, or edit pull requests in 1.0.

### Actions

The Actions page is scoped to a repository.

![Example Actions tab](https://github.com/user-attachments/assets/99db7d88-deef-413b-84b0-cae3ba04f7b3)

Users can:

* List workflows and recent runs.
* View run, job, and step status.
* Open logs on GitHub.
* Download available artifacts.
* Rerun failed jobs after confirmation.

Workflow dispatch, run cancellation, deployment approval, workflow editing, and permission management stay on GitHub.

### Discussions

Discussions are scoped to a repository.

![Example discussions tab](https://github.com/user-attachments/assets/49807774-9bbf-4d0e-9a26-ff91b83857f4)

Users can:

* List and filter discussions.
* Read a discussion and its comments.
* Create a discussion in a selected category.
* Add a comment.
* Open the discussion on GitHub.

Answer marking, pinning, locking, editing, deletion, and moderation stay on GitHub.

### Agents (Preview)

The Agent Tasks API is in public preview. The extension labels this entire surface Preview and keeps it behind capability detection and a versioned API adapter.

![Example Agents tab](https://github.com/user-attachments/assets/cc5ff526-3d12-4cd6-8c87-204af1b3359c)

Users can:

* List their agent tasks across repositories.
* Filter tasks by repository and state.
* Start a task with a repository, prompt, base branch, model, optional custom agent, and pull request preference.
* Assign an existing issue to Copilot.
* View task state and the latest available status.
* Cancel a task after confirmation.
* Open the resulting pull request.

The page handles `queued`, `in_progress`, `completed`, `failed`, `idle`, `waiting_for_user`, `timed_out`, and `cancelled` states.

If an agent needs input that the API cannot represent, the extension opens the task on GitHub. An Agent API failure never blocks the rest of the extension.

### Codespaces

The Codespaces page shows Codespaces for the active account.

![Example Codespaces tab](https://github.com/user-attachments/assets/442e2568-567a-4041-803c-329415c952a9)

Users can:

* List and filter Codespaces.
* Create a Codespace after seeing a clear billing warning.
* Start or stop a Codespace.
* Open a Codespace in the browser.
* Open a Codespace in an installed stable or Insiders build of Visual Studio Code.

Deletion, rebuilds, machine changes, retention changes, and secret management stay on GitHub.

### Saved queries

Saved queries remain a power user feature. 1.0 fixes their reliability before adding more query features.

![Example saved queries tab](https://github.com/user-attachments/assets/4628a1e9-5efc-4f41-91ef-3fe8fd437283)

The extension:

* Validates query syntax before saving.
* Parses supported GitHub search URLs into a query without dropping qualifiers.
* Stores a normalized form for identity and caching while preserving the user's original text for display and editing.
* Updates every visible instance of a query after an edit.
* Confirms query deletion.
* Supports pinning a saved query to the top level.
* Supports import and export through a versioned JSON format.

## Search behavior

Search follows the user's current context.

From the hub, remote search covers repositories, issues, and pull requests available to the active account. Inside a repository, remote search applies to the current repository and current surface when the GitHub API supports it.

Notifications, Agent tasks, Codespaces, Actions, releases, and discussions use local filtering by default. A surface can add remote search later when GitHub provides a useful query API.

The UI shows whether results are local or remote when that difference could surprise the user.

## Authentication and accounts

GitHub Extension 1.0 requires the Command Palette authentication contract proposed in [PowerToys pull request 50002](https://github.com/microsoft/PowerToys/pull/50002), or its final equivalent.

Authentication uses the Toolkit `OAuthClient` and host redirect broker:

* Authorization Code with PKCE for interactive sign in.
* No client secret in the extension package.
* Refreshable tokens when the provider supports them.
* Credential Manager storage through the Toolkit token store.
* Capability detection with a clear minimum host version.

The extension no longer owns a custom URI protocol, OAuth state management, token exchange plumbing, or raw Credential Manager interop.

### Provider validation

Before implementation chooses a GitHub OAuth App or GitHub App registration, an authentication spike must verify the full permission matrix on GitHub.com and `ghe.com`.

The spike covers:

* User notifications.
* Private repository search.
* Organization resources protected by SSO.
* Issue and pull request writes.
* Action runs and artifacts.
* Agent tasks.
* Codespaces.
* Releases.
* Discussions.

The selected registration must support user attributed access to every required API without shipping a secret. Preference does not win this decision. Working permissions do.

### Account model

An account is identified by its GitHub host and stable user ID, not by login alone.

The extension:

* Supports multiple stored accounts.
* Has one explicit active account.
* Shows the active account and host in the hub and Settings.
* Never silently chooses the first credential returned by storage.
* Clears account scoped memory and reloads pages when the active account changes.
* Keeps caches separated by host and account.

### Sign in and reauthorization

Onboarding asks the user to choose GitHub.com or a supported `ghe.com` host, then starts the host managed sign in.

When access requires an application installation, organization approval, or an active SSO session, the extension explains which step is missing and opens the right GitHub page.

The extension requests reauthorization only when a token is expired, revoked, missing a required permission, or incompatible with the new authentication contract.

## Settings

Settings are available through a native Command Palette settings page.

1.0 settings include:

* Active account.
* Add or remove account.
* Show item details.
* Notification polling preference within GitHub's allowed interval.
* Saved query import and export.
* Telemetry consent.
* Open logs.

Sign out moves from a top level command to Settings once the hub ships. The existing command can remain during the Preview migration window, but it is not part of the final 1.0 top level set.

## Data and persistence

Configuration and API cache data have different jobs and use different storage.

### Configuration

Durable user configuration is stored in a versioned JSON document. It includes:

* Saved queries and their display names.
* Query type and normalized identity.
* Top level pin state.
* Pinned repositories.
* Feature preferences.
* Telemetry consent.

Credentials, tokens, account bindings, repository history, cached content, and account identifiers are not included in exported configuration.

Writes are atomic. The extension writes a temporary file, validates it, and replaces the previous file only after the new document is complete.

Import validates the entire document before making changes. The default behavior merges entries by stable identity and shows the user what will change. Replacing existing configuration is available only as a confirmed action. A failed import makes no partial changes.

### API cache

Disposable API data remains in SQLite. Cache keys include the active host and account.

The cache:

* Stores response data needed for fast page loads.
* Stores ETags, last modified values, and server polling intervals where available.
* Shows cached results first, then refreshes in the background.
* Expires data by feature specific rules.
* Can be deleted and rebuilt without losing user configuration.

## API architecture

The current data layer grew around issue search. 1.0 replaces that center with feature specific clients behind shared transport and account services.

```text
Command Palette pages
        |
Feature services
        |
GitHub API adapters
        |
Authenticated transport
        |
Active account session
```

### Shared services

The shared layer owns:

* Active account selection.
* Authenticated HTTP transport.
* GitHub host endpoint resolution.
* API version headers.
* Pagination.
* Conditional requests.
* Rate limit state.
* Cancellation.
* Common error mapping.
* Local cache access.

### Feature services

Notifications, repositories, issues, pull requests, Actions, Agents, Codespaces, releases, and discussions each have a focused service contract.

Octokit can remain behind an adapter where it covers the API well. APIs that Octokit does not support use the shared HTTP transport directly. UI pages do not call Octokit or construct raw HTTP requests.

Preview APIs live behind their own versioned adapter so a breaking change does not spread through the stable clients.

### Asynchronous loading

Network work never blocks the Command Palette UI thread.

Pages return a loading state immediately, start cancellable background work, and raise item changes when data arrives. New code does not use `.Result` or `.GetAwaiter().GetResult()` for API or storage work.

Leaving a page cancels work that is no longer useful. Switching accounts cancels all work for the previous account.

### Error model

The API layer returns typed failures for:

* Authentication required.
* Reauthorization required.
* Application installation required.
* SSO required.
* Permission denied.
* Feature unavailable.
* Resource not found.
* Rate limited.
* Network unavailable.
* Validation failed.
* GitHub service failure.

Pages show a short message and a useful action. They do not turn exceptions into an empty list or a success shaped result.

## Rate limits and performance

Every API response updates the account's known rate limit state. The extension respects GitHub polling and retry headers.

The extension should:

* Render a page shell or loading item within 200 milliseconds.
* Render cached results within 500 milliseconds on supported hardware.
* Avoid duplicate requests for the same account, feature, and query.
* Share in flight requests when two pages need the same data.
* Paginate large collections instead of loading everything.
* Show rate limit reset information when a request cannot continue.

These are user experience budgets, not an excuse to hide failed work. Slow or failed refreshes stay visible.

## Telemetry and diagnostics

Telemetry is on by default and requires explicit opt-out.

If enabled, telemetry can record:

* Feature entry counts.
* Command success or failure category.
* Coarse latency buckets.
* Authentication outcome category.
* Cache hit or miss.
* API capability availability.

Telemetry never records:

* Account names or IDs.
* Host names.
* Repository names or IDs.
* Query text.
* Agent prompts.
* Issue, pull request, discussion, release, or comment content.
* URLs, branches, file names, tokens, or authorization codes.

Local logs contain enough context to diagnose failures without containing tokens, authorization codes, PKCE verifiers, prompts, query text, or GitHub content. Error pages include an Open logs command.

## Migration from Preview

Migration is automatic and idempotent.

On first 1.0 launch, the extension:

1. Reads the existing persistent search database.
2. Converts saved queries and top level pin state to the versioned configuration model.
3. Validates the complete migrated document.
4. Writes the new configuration atomically.
5. Keeps the old database until migration succeeds.
6. Discards the old API cache and rebuilds it as needed.

Existing credentials remain available when they work with the new host authentication contract and required permissions. Otherwise, the extension keeps the migrated configuration and asks the user to sign in again.

Migration can be safely retried after an interruption. It never deletes the only valid copy of user configuration.

## Accessibility and localization

1.0 uses native Command Palette controls and supports:

* Keyboard navigation for every command and form.
* Screen reader names that include item type, state, and repository context.
* High contrast themes.
* Visible focus states.
* Text scaling without clipping titles or actions.
* Status that is not communicated by color alone.
* Localizable user facing text.

Labels and tags cannot cover or truncate the primary title. When space is limited, details move to the subtitle or details pane.

## Security

The extension follows these rules:

* No client secret ships in the package.
* Tokens stay in the extension process and Credential Manager.
* Authentication codes, tokens, and PKCE values are never logged.
* All write commands use the active account shown to the user.
* Cost bearing actions show a warning before execution.
* Bulk state changes and task cancellation require confirmation.
* Imported configuration is treated as untrusted input.
* Downloaded assets keep the file name supplied by GitHub only after path validation.
* External URLs are limited to the selected GitHub host and known editor protocols.

## Packaging and platform requirements

1.0:

* Targets .NET 10.
* Ships x64 and ARM64 packages.
* Uses the PowerToys dependency feed.
* Requires the first stable Command Palette host version that implements the authentication and tabs contracts.
* Removes Preview from the extension display name.
* Labels only Agents as Preview.
* Uses the approved official logo and store assets.
* Keeps development packages visually and technically distinct from release packages.

## Delivery order

### 1. Foundation

* Build on the merged PowerToys dependency feed and merge the .NET 10 update.
* Merge the saved query URL parsing fix.
* Add shared API transport, typed errors, pagination, and rate limit tracking.
* Separate configuration from API cache storage.

### 2. Authentication and accounts

* Adopt the new Command Palette authentication contract.
* Complete the provider permission spike.
* Add GitHub.com and `ghe.com` host resolution.
* Add multiple accounts with one active account.
* Add onboarding, installation, SSO, and reauthorization guidance.

### 3. Hub and core work

* Add the consolidated GitHub hub.
* Add Notifications and Repositories.
* Rebuild Issues and Pull requests on the new service layer.
* Preserve existing top level searches and saved query pins.

### 4. Repository workflows

* Add Actions.
* Add Codespaces.
* Add Releases.
* Add Discussions.

### 5. Agents Preview

* Add Agent API capability detection.
* Add task creation, listing, status, cancellation, issue assignment, and pull request handoff.
* Confirm that Agent failures stay isolated from stable features.

### 6. Release hardening

* Complete Preview data migration.
* Complete accessibility and localization review.
* Add official branding.
* Add opt in telemetry and redacted diagnostics.
* Update installation, onboarding, and troubleshooting documentation.
* Validate x64 and ARM64 packaging.

## Release gates

1.0 is ready when:

* The required Command Palette authentication feature is available in a stable host release.
* GitHub.com and supported `ghe.com` sign in pass the permission matrix.
* Private repository and organization SSO paths have been tested.
* Preview saved queries and pin state migrate without data loss.
* Authentication, network, permission, and rate limit failures provide a useful recovery action.
* Optional or Preview APIs can fail without breaking stable surfaces.
* There are no known crashes, authentication lockouts, data loss bugs, or accessibility blockers.
* Core pages meet the loading and cache performance budgets.
* Unit, migration, API contract, and page behavior tests pass on x64 and ARM64.
* Telemetry remains off until a user explicitly opts in.
* Copilot Agents is clearly labeled Preview everywhere it appears.
* The package, documentation, icon, and display name no longer label the full extension Preview.

## Out of scope for 1.0

The following work can be considered for a 1.x release:

* Repository file browsing.
* User search.
* A global release feed.
* Pull request diff rendering and formal reviews.
* Pull request merge or close commands.
* Workflow dispatch and cancellation.
* Release publishing and editing.
* Discussion moderation.
* Codespace deletion and administration.
* Repository and organization administration.
* GitHub Enterprise Server on self hosted domains.
* Cross account aggregation.

## Backlog mapping

| Area | Issues | 1.0 direction |
| --- | --- | --- |
| Authentication and accounts | [#299](https://github.com/microsoft/CmdPalGitHubExtension/issues/299), [#284](https://github.com/microsoft/CmdPalGitHubExtension/issues/284), [#226](https://github.com/microsoft/CmdPalGitHubExtension/issues/226), [#217](https://github.com/microsoft/CmdPalGitHubExtension/issues/217), [#46](https://github.com/microsoft/CmdPalGitHubExtension/issues/46), [#45](https://github.com/microsoft/CmdPalGitHubExtension/issues/45) | New host authentication, account switching, private repository access, SSO, and `ghe.com` support |
| Hub and workflows | [#283](https://github.com/microsoft/CmdPalGitHubExtension/issues/283), [#262](https://github.com/microsoft/CmdPalGitHubExtension/issues/262), [#230](https://github.com/microsoft/CmdPalGitHubExtension/issues/230), [#65](https://github.com/microsoft/CmdPalGitHubExtension/issues/65), [#279](https://github.com/microsoft/CmdPalGitHubExtension/issues/279), [#208](https://github.com/microsoft/CmdPalGitHubExtension/issues/208), [#206](https://github.com/microsoft/CmdPalGitHubExtension/issues/206), [#60](https://github.com/microsoft/CmdPalGitHubExtension/issues/60), [#47](https://github.com/microsoft/CmdPalGitHubExtension/issues/47) | Consolidated hub and repository pages |
| Releases and pull request utilities | [#281](https://github.com/microsoft/CmdPalGitHubExtension/issues/281), [#199](https://github.com/microsoft/CmdPalGitHubExtension/issues/199) | Repository releases, artifacts, and browser handoff |
| Search and configuration | [#276](https://github.com/microsoft/CmdPalGitHubExtension/issues/276), [#273](https://github.com/microsoft/CmdPalGitHubExtension/issues/273), [#266](https://github.com/microsoft/CmdPalGitHubExtension/issues/266), [#253](https://github.com/microsoft/CmdPalGitHubExtension/issues/253), [#248](https://github.com/microsoft/CmdPalGitHubExtension/issues/248), [#232](https://github.com/microsoft/CmdPalGitHubExtension/issues/232), [#164](https://github.com/microsoft/CmdPalGitHubExtension/issues/164), [#130](https://github.com/microsoft/CmdPalGitHubExtension/issues/130), [#129](https://github.com/microsoft/CmdPalGitHubExtension/issues/129) | Validation, normalization, migration, import and export, and safer editing |
| Native experience and accessibility | [#267](https://github.com/microsoft/CmdPalGitHubExtension/issues/267), [#229](https://github.com/microsoft/CmdPalGitHubExtension/issues/229), [#222](https://github.com/microsoft/CmdPalGitHubExtension/issues/222), [#221](https://github.com/microsoft/CmdPalGitHubExtension/issues/221), [#219](https://github.com/microsoft/CmdPalGitHubExtension/issues/219), [#135](https://github.com/microsoft/CmdPalGitHubExtension/issues/135), [#126](https://github.com/microsoft/CmdPalGitHubExtension/issues/126), [#85](https://github.com/microsoft/CmdPalGitHubExtension/issues/85), [#78](https://github.com/microsoft/CmdPalGitHubExtension/issues/78), [#51](https://github.com/microsoft/CmdPalGitHubExtension/issues/51), [#48](https://github.com/microsoft/CmdPalGitHubExtension/issues/48), [#30](https://github.com/microsoft/CmdPalGitHubExtension/issues/30) | Native pages, details, settings, onboarding, diagnostics, and accessibility |
| Engineering and release | [#246](https://github.com/microsoft/CmdPalGitHubExtension/issues/246), [#220](https://github.com/microsoft/CmdPalGitHubExtension/issues/220), [#189](https://github.com/microsoft/CmdPalGitHubExtension/issues/189), [#93](https://github.com/microsoft/CmdPalGitHubExtension/issues/93), [#49](https://github.com/microsoft/CmdPalGitHubExtension/issues/49), [#41](https://github.com/microsoft/CmdPalGitHubExtension/issues/41), [#39](https://github.com/microsoft/CmdPalGitHubExtension/issues/39) | Branding, packaging, event cleanup, rate limits, opt in telemetry, and AOT readiness |

Issue [#67](https://github.com/microsoft/CmdPalGitHubExtension/issues/67) is deferred with user search. Issue [#297](https://github.com/microsoft/CmdPalGitHubExtension/issues/297) belongs to PowerToys Dock and should be transferred or closed here.

## Pull request disposition

* [#300](https://github.com/microsoft/CmdPalGitHubExtension/pull/300), PowerToys dependency feed: merged on 2026-08-19.
* [#298](https://github.com/microsoft/CmdPalGitHubExtension/pull/298), .NET 10: rebase after #300, then merge.
* [#282](https://github.com/microsoft/CmdPalGitHubExtension/pull/282), author URL parsing: refresh the stale branch, merge, and close #266.
* [#296](https://github.com/microsoft/CmdPalGitHubExtension/pull/296), Codespaces: keep the useful experience and tests, but rebuild it on the new authentication, asynchronous loading, error, and service architecture.
