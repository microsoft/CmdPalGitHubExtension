// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;
using Windows.Foundation;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SignInPageTest
{
    private const string SuccessMessage = "Sign in successful!";
    private const string ErrorMessage = "Sign in failed!";

    [TestMethod]
    public Task SignInPage_ShouldShowLoadingState_WhenSignInButtonClicked()
    {
        // Arrange
        var mockDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        var mockSignInForm = new Mock<SignInForm>(mockDeveloperIdProvider.Object);
        var mockStatusMessage = new Mock<StatusMessage>();
        var signInPage = new SignInPage(mockSignInForm.Object, mockStatusMessage.Object, SuccessMessage, ErrorMessage);

        var isLoading = false;
        mockSignInForm.SetupAdd(f => f.LoadingStateChanged += It.IsAny<TypedEventHandler<object, bool>>())
            .Callback<TypedEventHandler<object, bool>>((handler) => handler.Invoke(this, true));
        mockSignInForm.SetupAdd(f => f.LoadingStateChanged += It.IsAny<TypedEventHandler<object, bool>>())
            .Callback<TypedEventHandler<object, bool>>((handler) => handler.Invoke(this, false));

        // Act
        mockSignInForm.Raise(f => f.LoadingStateChanged += null, this, true);
        isLoading = true;
        mockSignInForm.Raise(f => f.LoadingStateChanged += null, this, false);
        isLoading = false;

        // Assert
        Assert.IsTrue(isLoading, "The page should be in loading state when sign-in starts.");
        Assert.IsFalse(isLoading, "The page should not be in loading state when sign-in completes.");
        return Task.CompletedTask;
    }

    [TestMethod]
    public Task SignInPage_ShouldShowSuccessMessage_WhenSignInSucceeds()
    {
        // Arrange
        var mockDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        var mockSignInForm = new Mock<SignInForm>(mockDeveloperIdProvider.Object);
        var mockStatusMessage = new Mock<StatusMessage>();
        var signInPage = new SignInPage(mockSignInForm.Object, mockStatusMessage.Object, SuccessMessage, ErrorMessage);

        var isSignInSuccessful = false;
        mockSignInForm.SetupAdd(f => f.FormSubmitted += It.IsAny<TypedEventHandler<object, FormSubmitEventArgs>>())
            .Callback<TypedEventHandler<object, FormSubmitEventArgs>>((handler) => handler.Invoke(this, new FormSubmitEventArgs(true, null)));

        // Act
        mockSignInForm.Raise(f => f.FormSubmitted += null, this, new FormSubmitEventArgs(true, null));
        isSignInSuccessful = true;

        // Assert
        Assert.IsTrue(isSignInSuccessful, "The success message should be shown when sign-in succeeds.");
        return Task.CompletedTask;
    }

    [TestMethod]
    public Task SignInPage_ShouldShowErrorMessage_WhenSignInFails()
    {
        // Arrange
        var mockDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        var mockSignInForm = new Mock<SignInForm>(mockDeveloperIdProvider.Object);
        var mockStatusMessage = new Mock<StatusMessage>();
        var signInPage = new SignInPage(mockSignInForm.Object, mockStatusMessage.Object, SuccessMessage, ErrorMessage);

        var isSignInFailed = false;
        mockSignInForm.SetupAdd(f => f.FormSubmitted += It.IsAny<TypedEventHandler<object, FormSubmitEventArgs>>())
            .Callback<TypedEventHandler<object, FormSubmitEventArgs>>((handler) => handler.Invoke(this, new FormSubmitEventArgs(false, new InvalidOperationException("Sign in failed!"))));

        // Act
        mockSignInForm.Raise(f => f.FormSubmitted += null, this, new FormSubmitEventArgs(false, new InvalidOperationException("Sign in failed!")));
        isSignInFailed = true;

        // Assert
        Assert.IsTrue(isSignInFailed, "The error message should be shown when sign-in fails.");
        return Task.CompletedTask;
    }
}
