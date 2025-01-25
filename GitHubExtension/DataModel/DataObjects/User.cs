// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel.DataObjects;

public class User
{
    public long Id { get; set; } = -1;

    public string Login { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public override string ToString() => Login;

    public string Type { get; set; } = string.Empty;

    public static User CreateFromOctokitUser(Octokit.User user)
    {
        return new User
        {
            Id = user.Id,
            Login = user.Login,
            AvatarUrl = user.AvatarUrl ?? string.Empty,
            Type = user.Type.HasValue ? user.Type.Value.ToString() : string.Empty,
        };
    }
}
