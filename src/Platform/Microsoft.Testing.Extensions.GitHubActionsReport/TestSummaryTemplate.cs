// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

#pragma warning disable CA1305

internal sealed class TestSummaryTemplate
{
    public TestSummaryTemplate(string testSuite, string targetFramework, TestRunStatistics testRunStatistics, IReadOnlyList<TestNodeUpdateMessage> testResults)
    {
        TestSuite = testSuite;
        TargetFramework = targetFramework;
        TestRunStatistics = testRunStatistics;
        TestResults = testResults;
    }

    public string TestSuite { get; }

    public string TargetFramework { get; }

    public TestRunStatistics TestRunStatistics { get; }

    public IReadOnlyList<TestNodeUpdateMessage> TestResults { get; }

    public string Render()
    {
        var builder = new StringBuilder();
        builder.AppendLine("<details>");

        string overallOutcomeEmoji;
        if (TestRunStatistics.FailedTestCount > 0)
        {
            overallOutcomeEmoji = "🔴";
        }
        else if (TestRunStatistics.SkippedTestCount > 0)
        {
            overallOutcomeEmoji = "🟡";
        }
        else if (TestRunStatistics.TotalTestCount == 0)
        {
            overallOutcomeEmoji = "⚪️";
        }
        else
        {
            // All are passing.
            overallOutcomeEmoji = "🟢";
        }

        builder.AppendLine("    <summary>");
        builder.AppendLine($"        <b>{overallOutcomeEmoji} {TestSuite}</b> ({TargetFramework})");
        builder.AppendLine("    </summary>");

        // This adds a margin that is smaller than <br>
        builder.AppendLine("<p></p>");
        builder.AppendLine("    <table>");
        builder.AppendLine("        <th width=\"99999\">✓&nbsp;&nbsp;Passed</th>");
        builder.AppendLine("        <th width=\"99999\">✘&nbsp;&nbsp;Failed</th>");
        builder.AppendLine("        <th width=\"99999\">↷&nbsp;&nbsp;Skipped</th>");
        builder.AppendLine("        <th width=\"99999\">∑&nbsp;&nbsp;Total</th>");
        builder.AppendLine("        <th width=\"99999\">⧗&nbsp;&nbsp;Elapsed</th>");
        builder.AppendLine("        <tr>");
        builder.AppendLine("            <td align=\"center\">");
        builder.AppendLine($"                {(TestRunStatistics.PassedTestCount > 0 ? TestRunStatistics.PassedTestCount : "-")}");
        builder.AppendLine("            </td>");
        builder.AppendLine("            <td align=\"center\">");
        builder.AppendLine($"                {(TestRunStatistics.FailedTestCount > 0 ? TestRunStatistics.FailedTestCount : "-")}");
        builder.AppendLine("            </td>");
        builder.AppendLine("            <td align=\"center\">");
        builder.AppendLine($"                {(TestRunStatistics.SkippedTestCount > 0 ? TestRunStatistics.SkippedTestCount : "-")}");
        builder.AppendLine("            </td>");
        builder.AppendLine("            <td align=\"center\">");
        builder.AppendLine($"                {TestRunStatistics.TotalTestCount}");
        builder.AppendLine("            </td>");
        builder.AppendLine("            <td align=\"center\">");
        builder.AppendLine($"                {TestRunStatistics.OverallDuration.ToHumanString()}");
        builder.AppendLine("            </td>");
        builder.AppendLine("        </tr>");
        builder.AppendLine("    </table>");

#pragma warning disable IDE0037 // Use inferred member name - suggestion makes code less clear.
        var testResultGroups =
            TestResults.GroupBy(
                r =>
                {
                    TestMethodIdentifierProperty? identifierProperty = r.TestNode.Properties.OfType<TestMethodIdentifierProperty>().SingleOrDefault();
                    return identifierProperty is null
                        ? null
                        : $"{identifierProperty.Namespace}.{identifierProperty.TypeName}";
                }, StringComparer.Ordinal)
            .Select(g => new
            {
                TypeFullyQualifiedName = g.Key,
                TypeName = g.First().TestNode.Properties.OfType<TestMethodIdentifierProperty>().SingleOrDefault()?.TypeName,
                TestResults = g
                    .OrderByDescending(r => r.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>() is FailedTestNodeStateProperty)
                    .ThenByDescending(r => r.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>() is PassedTestNodeStateProperty)
                    .ThenBy(r => r.TestNode.DisplayName, StringComparer.Ordinal)
                    .ToArray(),
            });
#pragma warning restore IDE0037 // Use inferred member name

        builder.AppendLine("    <ul>");

        foreach (var testResultGroup in testResultGroups)
        {
            int failedTestCount = TestRunStatistics.FailedTestCount;
            builder.Append("        <li>");
            builder.Append($"            <b>{testResultGroup.TypeName}</b>");
            if (failedTestCount > 0)
            {
                builder.Append($"            <i>({failedTestCount} failed)</i>");
            }

            // This adds a margin that is smaller than <br>
            builder.AppendLine("            <p></p>");

            builder.AppendLine("            <ul>");
            foreach (TestNodeUpdateMessage? testResult in testResultGroup.TestResults)
            {
                TestNodeStateProperty? state = testResult.TestNode.Properties.OfType<TestNodeStateProperty>().SingleOrDefault();

                string outcomeEmoji = state switch
                {
                    PassedTestNodeStateProperty => "🟩",
                    FailedTestNodeStateProperty or TimeoutTestNodeStateProperty or ErrorTestNodeStateProperty => "🟥",
                    SkippedTestNodeStateProperty => "🟨",
                    _ => "\u2753",
                };

                string testName = testResult.TestNode.DisplayName;
                TestFileLocationProperty? location = testResult.TestNode.Properties.OfType<TestFileLocationProperty>().SingleOrDefault();

                // Test source permalink
                string? filePath = location?.FilePath;
                int? fileLine = location?.LineSpan.Start.Line;
                string? url = GitHubWorkflow.TryGenerateFilePermalink(filePath, fileLine);

                builder.AppendLine("                <li>");
                builder.AppendLine($"                    {outcomeEmoji}");

                if (!RoslynString.IsNullOrWhiteSpace(url))
                {
                    builder.AppendLine($"                        <a href=\"{url}\">{testName}</a>");
                }
                else
                {
                    builder.AppendLine($"                        {testName}");
                }

                Exception? ex = null;
                if (state is FailedTestNodeStateProperty failedState)
                {
                    ex = failedState.Exception;
                }
                else if (state is ErrorTestNodeStateProperty errorState)
                {
                    ex = errorState.Exception;
                }
                else if (state is TimeoutTestNodeStateProperty timeoutState)
                {
                    ex = timeoutState.Exception;
                }
                else if (state is CancelledTestNodeStateProperty cancelledState)
                {
                    ex = cancelledState.Exception;
                }

                if (ex is not null)
                {
                    builder.AppendLine();
                    builder.AppendLine("```yml");
                    builder.AppendLine(ex.Message);
                    builder.AppendLine(ex.StackTrace);
                    builder.AppendLine("```");
                    builder.AppendLine();
                }

                builder.AppendLine("                </li>");
            }

            builder.AppendLine("            </ul>");

            // This adds a margin that is smaller than <br>
            builder.AppendLine("            <p></p>");
            builder.Append("        </li>");
        }

        builder.AppendLine("    </ul>");
        builder.AppendLine("</details>");
        return builder.ToString();
    }
}
