// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Windows.ApplicationModel.Resources;
using Serilog;

namespace GitHubExtension.Helpers;

public static class Resources
{
    private static ResourceLoader? _resourceLoader;

    public static string GetResource(string identifier, ILogger? log = null)
    {
        try
        {
            if (_resourceLoader == null)
            {
                _resourceLoader = new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath());
            }

            return _resourceLoader.GetString(identifier);
        }
        catch (Exception ex)
        {
            log?.Error(ex, $"Failed loading resource: {identifier}");

            // If we fail, load the original identifier so it is obvious which resource is missing.
            return identifier;
        }
    }

    // Replaces all identifiers in the provided list in the target string. Assumes all identifiers
    // are wrapped with '%' to prevent sub-string replacement errors. This is intended for strings
    // such as a JSON string with resource identifiers embedded.
    public static string ReplaceIdentifiers(string str, string[] resourceIdentifiers, ILogger? log = null)
    {
        var start = DateTime.Now;
        foreach (var identifier in resourceIdentifiers)
        {
            // What is faster, String.Replace, RegEx, or StringBuilder.Replace? It is String.Replace().
            // https://learn.microsoft.com/archive/blogs/debuggingtoolbox/comparing-regex-replace-string-replace-and-stringbuilder-replace-which-has-better-performance
            var resourceString = GetResource(identifier, log);
            str = str.Replace($"%{identifier}%", resourceString);
        }

        var elapsed = DateTime.Now - start;
        log?.Debug($"Replaced identifiers in {elapsed.TotalMilliseconds}ms");
        return str;
    }

    // These are all the string identifiers that appear in widgets.
    public static string[] GetWidgetResourceIdentifiers()
    {
        return new string[]
        {
            "Widget_Template/ContentLoading",
            "Widget_Template/EmptyIssues",
            "Widget_Template/EmptyPulls",
            "Widget_Template/EmptyAssigned",
            "Widget_Template/EmptyMentioned",
            "Widget_Template/EmptyReviews",
            "Widget_Template/EmptyReleases",
            "Widget_Template/Pulls",
            "Widget_Template/Issues",
            "Widget_Template/Opened",
            "Widget_Template/By",
            "Widget_Template/SignInRequired",
            "Extension_Name/GitHub",
            "Widget_Template/Repository",
            "Widget_Template/Label",
            "Widget_Template/Loading",
            "Widget_Template/Author",
            "Widget_Template/Milestone",
            "Widget_Template/Project",
            "Widget_Template/PullRequests",
            "Widget_Template_Label/Url",
            "Widget_Template_Tooltip/Submit",
            "Widget_Template_Button/Submit",
            "Widget_Template_Tooltip/OpenPullRequest",
            "Widget_Template_Input/UrlPlaceholder",
            "Widget_Template_Tooltip/OpenIssue",
            "Widget_Template_Button/SignIn",
            "Widget_Template_Tooltip/SignIn",
            "Widget_Template/Assigned",
            "Widget_Template/AssignedTo",
            "Widget_Template/AssignedTitle",
            "Widget_Template/Mentioned",
            "Widget_Template/MentionedLow",
            "Widget_Template/Mentioned_user",
            "Widget_Template/ShowInWidget",
            "Widget_Template/IssuesAndPullRequests",
            "Widget_Template/Open",
            "Widget_Template/SelectInfoText",
            "Widget_Template/PR_Issue_description",
            "Widget_Template/ReviewRequestedFrom",
            "Widget_Template/ReviewRequestedTitle",
            "Widget_Template/PR_description",
            "Widget_Template/PR_info",
            "Widget_Template/Updated",
            "Widget_Template/Search",
            "Widget_Template_Button/Save",
            "Widget_Template_Button/Cancel",
            "Widget_Template_Tooltip/Save",
            "Widget_Template_Tooltip/Cancel",
            "Widget_Template/ChooseAccountPlaceholder",
            "Widget_Template/Published",
            "Widget_Template_Tooltip/OpenRelease",
            "Widget_Template/WidgetTitleLabel",
            "Widget_Template/WidgetTitlePlaceholder",
            "SignIn_Title",
        };
    }
}
