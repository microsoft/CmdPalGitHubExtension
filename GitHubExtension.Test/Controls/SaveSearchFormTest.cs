// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls.Forms;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SaveSearchFormTest
{
    [TestMethod]
    public void CreateSearchFromJson_ShouldSetIsTopLevel()
    {
        // Arrange
        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""test search"",
                ""Name"": ""test name"",
                ""IsTopLevel"": ""true""
            }");

        // Act
        var searchCandidate = SaveSearchForm.CreateSearchFromJson(jsonPayload);

        // Assert
        Assert.IsNotNull(searchCandidate);
        Assert.AreEqual("test search", searchCandidate.SearchString);
        Assert.AreEqual("test name", searchCandidate.Name);
        Assert.IsTrue(searchCandidate.IsTopLevel);
    }
}
