// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DataManager;
using GitHubExtension.DeveloperId;
using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Extensions.Configuration;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.Storage;
using Serilog;
using Windows.ApplicationModel.Activation;

namespace GitHubExtension;

public class Program
{
    private static DeveloperIdProvider? _developerIdProvider;

    [MTAThread]
    public static async Task Main(string[] args)
    {
        // Setup Logging
        Environment.SetEnvironmentVariable("CMDPAL_LOGS_ROOT", ApplicationData.GetDefault().TemporaryFolder.Path);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Information($"Launched with args: {string.Join(' ', args.ToArray())}");

        // Force the app to be single instanced.
        // Get or register the main instance.
        var mainInstance = AppInstance.FindOrRegisterForKey("mainInstance");
        var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        if (!mainInstance.IsCurrent)
        {
            Log.Information($"Not main instance, redirecting.");
            await mainInstance.RedirectActivationToAsync(activationArgs);
            Log.CloseAndFlush();
            return;
        }

        // Register for activation redirection.
        AppInstance.GetCurrent().Activated += AppActivationRedirected;

        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            try
            {
                HandleCOMServerActivation();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register process as COM server.");
            }
        }
        else
        {
            Console.WriteLine("Not being launched as a Extension... exiting.");
        }
    }

    private static void AppActivationRedirected(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments activationArgs)
    {
        Log.Information($"Redirected with kind: {activationArgs.Kind}");

        // Handle COM server.
        if (activationArgs.Kind == ExtendedActivationKind.Launch)
        {
            var d = activationArgs.Data as ILaunchActivatedEventArgs;
            var args = d?.Arguments.Split();

            if (args?.Length > 1 && args[1] == "-RegisterProcessAsComServer")
            {
                Log.Information($"Activation COM Registration Redirect: {string.Join(' ', args.ToList())}");
                HandleCOMServerActivation();
            }
        }

        // Handle Protocol.
        if (activationArgs.Kind == ExtendedActivationKind.Protocol)
        {
            var d = activationArgs.Data as IProtocolActivatedEventArgs;
            if (d is not null)
            {
                Log.Information($"Protocol Activation redirected from: {d.Uri}");
                HandleProtocolActivation(d.Uri);
            }
        }
    }

    private static void HandleCOMServerActivation()
    {
        using ExtensionServer server = new();
        var extensionDisposedEvent = new ManualResetEvent(false);

        // COMPOSITION ROOT AREA
        var developerIdProvider = new DeveloperIdProvider();
        _developerIdProvider = developerIdProvider;

        var gitHubClientProvider = new GitHubClientProvider(developerIdProvider);

        using var gitHubDataManager = new GitHubDataManager(developerIdProvider, gitHubClientProvider);

        ISearchRepository searchRepository = new PersistentDataManager(developerIdProvider);

        using var cacheManager = new CacheManager(gitHubDataManager, searchRepository)!;

        // Set up cache manager to pre-update data
        cacheManager.Start();

        ICacheDataManager cacheDataManager = new CacheDataManagerFacade(cacheManager, gitHubDataManager);

        var searchPageFactory = new SearchPageFactory(cacheDataManager, searchRepository);

        var addSearchListItem = new AddSearchListItem(new SaveSearchPage(new SaveSearchForm(SearchInput.SearchString, searchRepository), new StatusMessage(), "Search saved successfully!", "Error in saving search"));
        var addSearchFullFormListItem = new AddSearchFullFormListItem(new SaveSearchPage(new SaveSearchForm(SearchInput.Survey, searchRepository), new StatusMessage(), "Search saved successfully!", "Error in saving search"));

        var savedSearchesPage = new SavedSearchesPage(searchPageFactory, searchRepository, addSearchListItem, addSearchFullFormListItem);

        var signOutPage = new SignOutPage(new SignOutForm(developerIdProvider), new StatusMessage(), "Sign out succeeded!", "Sign out failed");
        var signInPage = new SignInPage(new SignInForm(developerIdProvider), new StatusMessage(), "Sign in succeeded!", "Sign in failed");

        var commandProvider = new GitHubExtensionCommandsProvider(savedSearchesPage, signOutPage, signInPage, developerIdProvider);
        var extensionInstance = new GitHubExtension(extensionDisposedEvent, commandProvider);

        // We are instantiating an extension instance once above, and returning it every time the callback in RegisterExtension below is called.
        // This makes sure that only one instance of GitHubExtension is alive, which is returned every time the host asks for the IExtension object.
        // If you want to instantiate a new instance each time the host asks, create the new instance inside the delegate.
        server.RegisterExtension(() => extensionInstance);

        // END OF COMPOSITION ROOT AREA

        // This will make the main thread wait until the event is signalled by the extension class.
        // Since we have single instance of the extension object, we exit as soon as it is disposed.
        extensionDisposedEvent.WaitOne();
    }

    private static void HandleProtocolActivation(Uri oauthRedirectUri) => _developerIdProvider?.HandleOauthRedirection(oauthRedirectUri);
}
