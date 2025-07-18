# Welcome to the Command Palette (CmdPal) GitHub Extension (Preview) repo

This repository contains the source code for:

* [Command Palette GitHub Extension](https://github.com/microsoft/CmdPalGitHubExtension)

Related repositories include:

* [PowerToys Command Palette utility](https://github.com/microsoft/PowerToys/tree/main/src/modules/cmdpal)

## Installing and running Command Palette GitHub Extension (Preview)

### Requirements
The Command Palette GitHub Extension requires:
* PowerToys with Command Palette included
* Windows 11
* An ARM64 or x64 processor

## Command Palette [Recommended]

* In Command Palette, search for "Install Command Palette extensions" and press Enter.
* Search for "Command Palette GitHub Extension (Preview)".
* Press enter to download the extension.
* Navigate back to the main screen. If the extension was able to detect your account automatically, you will see the following commands:
    * Saved queries
    * Assigned to me
    * Created issues
    * My pull requests
    * Mentions me
* Otherwise, you'll see a "Sign in" command. Click sign in to login via the GitHub OAuth flows.

### WinGet

More instructions coming soon!

### Microsoft Store

The Command Palette GitHub Extension is coming to the Microsoft Store. Stay tuned for updates and instructions.

### Other install methods

#### Via GitHub

For users who are unable to install the Command Palette GitHub Extension from the winget or the Microsoft Store, released builds can be manually downloaded from this repository's [Releases page](https://github.com/microsoft/CmdPalGitHubExtension/releases).

## Using the extension

Check out this [quick start guide](docs/quickstart.md) to get started.

---

## Contributing

We are excited to work alongside you, our amazing community, to build and enhance the Command Palette GitHub Extension!

## Communicating with the team

The easiest way to communicate with the team is via GitHub issues.

Please file new issues, feature requests and suggestions, but **DO search for similar open/closed preexisting issues before creating a new issue.**

Add this to your Saved Queries: *repo:microsoft/CmdPalGitHubExtension is:issue*

If you would like to ask a question that you feel doesn't warrant an issue (yet), please reach out to us via X or Bluesky:

* Kayla Cinnamon, Senior Product Manager: [@cinnamon_msft on X](https://twitter.com/cinnamon_msft), [@kaylacinnamon on Bluesky](https://bsky.app/profile/kaylacinnamon.bsky.social)
* Clint Rutkas, Principal Product Manager: [@clintrutkas on X](https://twitter.com/clintrutkas), [@clintrutkas on Bluesky](https://bsky.app/profile/clintrutkas.bsky.social)

## Building the code

* Clone the repository
* Uninstall the Command Palette GitHub Extension (Command Palette has a hard time choosing which extension to use if two versions exist)
* Open `GitHubExtension.sln` in Visual Studio 2022 or later and build from the IDE, or run `build\scripts\Build.ps1` from a Visual Studio command prompt.

### OAuth App
Since secrets cannot be checked in to the repository, developers must create their own test OAuth app for local tests.

Follow this link https://docs.github.com/en/developers/apps/building-oauth-apps/creating-an-oauth-app to create a Git OAuth app (with RedirectUri = "cmdpalghext://oauth_redirect_uri/").

The OAuth App ClientId and ClientSecret can be added as environment variables using the following instructions:

    On an elevated cmd window:
        setx GITHUB_CLIENT_ID "Your OAuth App's ClientId" /m
        setx GITHUB_CLIENT_SECRET "Your OAuth App's ClientSecret" /m

---

## Code of Conduct

We welcome contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
