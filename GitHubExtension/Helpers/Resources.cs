// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Windows.ApplicationModel.Resources;
using Serilog;

namespace GitHubExtension.Helpers;

public class Resources : IResources
{
    private readonly ResourceLoader _resourceLoader;

    public Resources(ResourceLoader resourceLoader)
    {
        _resourceLoader = resourceLoader;
    }

    public string GetResource(string identifier, ILogger? log = null)
    {
        try
        {
            return _resourceLoader.GetString(identifier);
        }
        catch (Exception ex)
        {
            log?.Error(ex, $"Failed loading resource: {identifier}");

            // If we fail, load the original identifier so it is obvious which resource is missing.
            return identifier;
        }
    }
}

public interface IResources
{
    string GetResource(string identifier, ILogger? log = null);
}
