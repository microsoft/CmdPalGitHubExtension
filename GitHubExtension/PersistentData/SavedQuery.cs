// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper.Contrib.Extensions;

namespace GitHubExtension.PersistentData;

[Table("SavedQuery")]
public class SavedQuery
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;
}
