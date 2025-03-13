// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Serilog;
using Serilog.Configuration;

namespace GitHubExtension.Test.TestContextSink;

public static class TestContextSinkExtensions
{
    public static LoggerConfiguration TestContextSink(
              this LoggerSinkConfiguration loggerConfiguration,
              string outputTemplate,
              TestContext? context = null,
              IFormatProvider? formatProvider = null)
    {
        return loggerConfiguration.Sink(new TestContextSink(formatProvider!, context!, outputTemplate));
    }
}
