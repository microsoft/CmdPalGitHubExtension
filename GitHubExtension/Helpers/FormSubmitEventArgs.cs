// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

public class FormSubmitEventArgs : EventArgs
{
    // Status can mean success or failure as well as signed in or signed out
    public bool Status { get; }

    public Exception? Exception { get; }

    public FormSubmitEventArgs(bool status, Exception ex)
    {
        Status = status;
        Exception = ex;
    }
}
