// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Pages;

public interface ILabel
{
    string Name { get; }

    string Color { get; }

    Microsoft.CommandPalette.Extensions.Color GetFontColor();
}
