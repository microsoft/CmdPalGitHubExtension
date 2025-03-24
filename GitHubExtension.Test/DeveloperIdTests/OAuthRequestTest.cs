// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using Moq;
using Octokit;

namespace GitHubExtension.Test.DeveloperIdTests;

[TestClass]
public class OAuthRequestTest
{
    [TestMethod]
    public async Task CompleteOAuthAsync_ShouldThrowInvalidOperationException_OnTimeout()
    {
        var mockGitHubClient = new Mock<IGitHubClient>();
        var mockOauthClient = new Mock<IOauthClient>();

        mockGitHubClient.Setup(client => client.Oauth).Returns(mockOauthClient.Object);

        mockOauthClient
            .Setup(oauth => oauth.CreateAccessToken(It.IsAny<OauthTokenRequest>()))
            .Returns(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return new OauthToken();
            });

        var oAuthRequest = new OAuthRequest();

        var authorizationResponse = new Uri("https://example.com/callback?code=valid_code");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await oAuthRequest.CompleteOAuthAsync(authorizationResponse);
        });
    }
}
