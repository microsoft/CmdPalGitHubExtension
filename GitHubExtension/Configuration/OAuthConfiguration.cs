// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension;

internal static class OauthConfiguration
{
    // This redirect url has to be configured into the OAuth app. This package has "cmdpalghext://"
    // protocol extension and will receive the access token in this uri after successful login.
    internal const string RedirectUri = "cmdpalghext://oauth_redirect_uri/";

    // This class is to inject secrets at build-time
    // DO NOT MODIFY THESE VALUES.
    // USE DeveloperOAuthConfiguration.cs FOR LOCAL TESTS.
    private static class BuildTimeReplacements
    {
        internal static readonly string ClientID = "%BUILD_TIME_GITHUB_CLIENT_ID_PLACEHOLDER%";

        internal static readonly string ClientSecret = "%BUILD_TIME_GITHUB_CLIENT_SECRET_PLACEHOLDER%";
    }

    public static string GetClientId()
    {
        if (BuildTimeReplacements.ClientID.Equals("%" + "BUILD_TIME_GITHUB_CLIENT_ID_PLACEHOLDER" + "%", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(DeveloperOAuthConfiguration.ClientID))
            {
                // Throw if neither the Build-time constant or the environment variable is set.
                throw new InvalidOperationException("ClientID has not been set.");
            }

            return DeveloperOAuthConfiguration.ClientID;
        }
        else
        {
            return BuildTimeReplacements.ClientID;
        }
    }

    public static string GetClientSecret()
    {
        if (BuildTimeReplacements.ClientSecret.Equals("%" + "BUILD_TIME_GITHUB_CLIENT_SECRET_PLACEHOLDER" + "%", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(DeveloperOAuthConfiguration.ClientSecret))
            {
                // Throw if neither the Build-time constant or the environment variable is set.
                throw new InvalidOperationException("ClientSecret has not been set.");
            }

            return DeveloperOAuthConfiguration.ClientSecret;
        }
        else
        {
            return BuildTimeReplacements.ClientSecret;
        }
    }
}
