// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal sealed class TestReporterContext(GitHubWorkflow github, TestReporterOptions options)
{
    private readonly List<TestNodeUpdateMessage> _messagesOfInterest = [];
    private readonly Stopwatch _stopwatch = new();

    private int _passedCount;
    private int _failedCount;
    private int _skippedCount;

    public TestReporterOptions Options { get; } = options;

    private static string FormatAnnotation(string format, TestNode testNode, TestNodeStateProperty testNodeStateProperty)
    {
        var buffer = new StringBuilder(format);

        // Escaped new line token (backwards compat)
        buffer.Replace("\\n", "\n");

        // Name token
        buffer
            .Replace("@test", testNode.DisplayName)
            // Backwards compat
            .Replace("$test", testNode.DisplayName);

        // Trait tokens
        foreach (TestMetadataProperty metadataProperty in testNode.Properties.OfType<TestMetadataProperty>())
        {
            buffer
                .Replace($"@traits.{metadataProperty.Key}", metadataProperty.Value)
                // Backwards compat
                .Replace($"$traits.{metadataProperty.Key}", metadataProperty.Value);
        }

        Exception? exception = testNodeStateProperty switch
        {
            FailedTestNodeStateProperty failed => failed.Exception,
            ErrorTestNodeStateProperty error => error.Exception,
            _ => null,
        };

        // Error message
        buffer
            .Replace("@error", exception?.Message ?? string.Empty)
            // Backwards compat
            .Replace("$error", exception?.Message ?? string.Empty);

        // Error trace
        buffer
            .Replace("@trace", exception?.StackTrace ?? string.Empty)
            // Backwards compat
            .Replace("$trace", exception?.StackTrace ?? string.Empty);

        // Target framework
        // TODO: Copy logic from platform: https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform/OutputDevice/BrowserOutputDevice.cs#L78
        // or ask for platform to expose it
        buffer
            .Replace("@framework", string.Empty)
            // Backwards compat
            .Replace("$framework", string.Empty);

        return buffer.Trim().ToString();
    }

    private string FormatAnnotationTitle(TestNode testNode, TestNodeStateProperty testNodeStateProperty) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testNode, testNodeStateProperty);

    private string FormatAnnotationMessage(TestNode testNode, TestNodeStateProperty testNodeStateProperty) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testNode, testNodeStateProperty);

    public void HandleTestResult(TestNodeUpdateMessage testNodeUpdateMessage)
    {
        TestNodeStateProperty testNodeState = testNodeUpdateMessage.TestNode.Properties.Single<TestNodeStateProperty>();
        switch (testNodeState)
        {
            case PassedTestNodeStateProperty:
                _passedCount++;
                if (Options.SummaryIncludePassedTests)
                {
                    _messagesOfInterest.Add(testNodeUpdateMessage);
                }

                break;

            case SkippedTestNodeStateProperty:
                _skippedCount++;
                if (Options.SummaryIncludeSkippedTests)
                {
                    _messagesOfInterest.Add(testNodeUpdateMessage);
                }

                break;

            case CancelledTestNodeStateProperty:
            case ErrorTestNodeStateProperty:
            case FailedTestNodeStateProperty:
            case TimeoutTestNodeStateProperty:
                _failedCount++;
                _messagesOfInterest.Add(testNodeUpdateMessage);
                break;
        }

        // Report failed test results to job annotations
        if (testNodeState is FailedTestNodeStateProperty or ErrorTestNodeStateProperty)
        {
            TestFileLocationProperty? location = testNodeUpdateMessage.TestNode.Properties.SingleOrDefault<TestFileLocationProperty>();
            github.CreateErrorAnnotation(
                FormatAnnotationTitle(testNodeUpdateMessage.TestNode, testNodeState),
                FormatAnnotationMessage(testNodeUpdateMessage.TestNode, testNodeState),
                location?.FilePath,
                location?.LineSpan.Start.Line);
        }
    }

    public void HandleTestRunStart()
        => _stopwatch.Start();

    public void HandleTestRunComplete()
    {
        _stopwatch.Stop();

        string testSuite = Assembly.GetEntryAssembly()?.GetName().Name
            ?? "Unknown Test Suite";

        string targetFramework =
            // See line 65
            "Unknown Target Framework";

        var testRunStatistics = new TestRunStatistics(
            _passedCount,
            _failedCount,
            _skippedCount,
            _passedCount + _failedCount + _skippedCount,
            _stopwatch.Elapsed);

        var template = new TestSummaryTemplate(
            testSuite,
            targetFramework,
            testRunStatistics,
            _messagesOfInterest);

        github.CreateSummary(template.Render());
    }
}
