// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperIds;
using Moq;
using Octokit;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SignInFormTest
{
    [TestMethod]
    public async Task HandleOAuthRedirection_ShouldThrowInvalidOperationException_OnTimeout()
    {
        var mockGitHubClient = new Mock<IGitHubClient>();
        var mockOauthClient = new Mock<IOauthClient>();

        mockGitHubClient.Setup(client => client.Oauth).Returns(mockOauthClient.Object);

        mockOauthClient
            .Setup(oauth => oauth.CreateAccessToken(It.IsAny<OauthTokenRequest>()))
            .Returns(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10)); // Simulate delay longer than timeout
                return new OauthToken();
            });

        var mockCredentialVault = new Mock<ICredentialVault>();
        var developerIdProvider = new DeveloperIdProvider(mockCredentialVault.Object);

        mockGitHubClient.Setup(client => client.Oauth).Returns(mockOauthClient.Object);

        mockOauthClient
            .Setup(oauth => oauth.CreateAccessToken(It.IsAny<OauthTokenRequest>()))
            .Returns(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10)); // Simulate delay longer than timeout
                return new OauthToken();
            });

        var oAuthRequest = new OAuthRequest();

        var authorizationResponse = new Uri("https://example.com/callback?code=valid_code");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await Task.Run(() => developerIdProvider.HandleOauthRedirection(authorizationResponse));
        });
    }
}
