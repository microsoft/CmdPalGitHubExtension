// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.ListItems;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Extensions.Configuration;
using Microsoft.Windows.ApplicationModel.Resources;
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

        var path = ResourceLoader.GetDefaultResourceFilePath();
        var resourceLoader = new ResourceLoader(path);
        var resources = new Resources(resourceLoader);

        var gitHubClientProvider = new GitHubClientProvider(developerIdProvider);

        using var gitHubDataManager = new GitHubDataManager(gitHubClientProvider);

        using var searchRepository = new PersistentDataManager(new GitHubValidatorAdapter(gitHubClientProvider));

        using var cacheManager = new CacheManager(new GitHubCacheAdapter(gitHubDataManager), searchRepository)!;

        // Set up cache manager to pre-update data
        cacheManager.Start();

        var decoratorFactory = new DecoratorFactory(gitHubDataManager);
        var cacheDataManager = new CacheDataManagerFacade(cacheManager, gitHubDataManager, decoratorFactory);

        var searchPageFactory = new SearchPageFactory(cacheDataManager, searchRepository, resources);

        var addSearchListItem = new AddSearchListItem(new SaveSearchPage(new SaveSearchForm(searchRepository, resources), new StatusMessage(), resources.GetResource("Message_Search_Saved"), resources.GetResource("Message_Search_Saved_Error"), resources.GetResource("ListItems_AddSearch")), resources);

        var savedSearchesPage = new SavedSearchesPage(searchPageFactory, searchRepository, resources, addSearchListItem);

        var signOutPage = new SignOutPage(new SignOutForm(developerIdProvider, resources), new StatusMessage(), resources.GetResource("Message_Sign_Out_Success"), resources.GetResource("Message_Sign_Out_Fail"));
        var signInPage = new SignInPage(new SignInForm(developerIdProvider, resources), new StatusMessage(), resources.GetResource("Message_Sign_In_Success"), resources.GetResource("Message_Sign_In_Fail"));

        var commandProvider = new GitHubExtensionCommandsProvider(savedSearchesPage, signOutPage, signInPage, developerIdProvider, searchRepository, resources, searchPageFactory);
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
