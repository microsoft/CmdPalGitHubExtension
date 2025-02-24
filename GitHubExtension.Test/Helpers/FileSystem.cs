// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Test;

public class FileSystem
{
    public static string BuildOutputFilename(string filename, string outputFolder, bool createPathIfNecessary = true)
    {
        var outputFilename = SubstituteOutputFilename(filename, outputFolder);
        var file = new FileInfo(outputFilename);
        if (createPathIfNecessary)
        {
            file.Directory?.Create();
        }

        return file.FullName;
    }

    public static string SubstituteNow(string s)
    {
        if (s.Contains("{now}", StringComparison.CurrentCulture))
        {
            var now = DateTime.Now;
            var nowAsString = $"{now:yyyyMMdd-HHmmss}";
            return s.Replace("{now}", nowAsString);
        }

        return s;
    }

    public static string SubstituteOutputFilename(string filename, string outputDirectory)
    {
        return Path.Combine(SubstituteNow(outputDirectory), SubstituteNow(filename));
    }
}
