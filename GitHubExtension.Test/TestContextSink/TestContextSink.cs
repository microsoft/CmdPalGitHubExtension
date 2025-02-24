// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace GitHubExtension.Test;

public class TestContextSink : ILogEventSink
{
    private readonly TestContext? _testContext;

    private readonly MessageTemplateTextFormatter _formatter;

    public TestContextSink(IFormatProvider formatProvider, TestContext testContext, string outputTemplate)
    {
        _testContext = testContext;
        _formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
    }

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        _formatter.Format(logEvent, writer);
        _testContext?.Write(writer.ToString());
    }
}
