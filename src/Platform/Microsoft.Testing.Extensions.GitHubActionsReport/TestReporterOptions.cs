// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.CommandLine;

using static Microsoft.Testing.Extensions.GitHubActionsReport.CliOptionsProvider;

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal sealed class TestReporterOptions
{
    public TestReporterOptions(
        string annotationTitleFormat,
        string annotationMessageFormat,
        bool summaryIncludePassedTests,
        bool summaryIncludeSkippedTests)
    {
        AnnotationTitleFormat = annotationTitleFormat;
        AnnotationMessageFormat = annotationMessageFormat;
        SummaryIncludePassedTests = summaryIncludePassedTests;
        SummaryIncludeSkippedTests = summaryIncludeSkippedTests;
    }

    public string AnnotationTitleFormat { get; }

    public string AnnotationMessageFormat { get; }

    public bool SummaryIncludePassedTests { get; }

    public bool SummaryIncludeSkippedTests { get; }

    public static TestReporterOptions Resolve(ICommandLineOptions commandLineOptions)
    {
        string annotationTitleFormat =
            (commandLineOptions.TryGetOptionArgumentList(ReportGitHubTitleOption, out string[]? arguments)
                ? arguments[0]
                : null)
            ?? "@test";

        string annotationMessageFormat =
            (commandLineOptions.TryGetOptionArgumentList(ReportGitHubMessageOption, out arguments)
                ? arguments[0]
                : null)
            ?? "@error";

        _ = commandLineOptions.TryGetOptionArgumentList(ReportGitHubSummaryOption, out arguments);

        bool includePassedTests =
            arguments?.Contains(ReportGitHubSummaryArguments.IncludePassedTests) == true;

        bool includeSkippedTests =
            arguments?.Contains(ReportGitHubSummaryArguments.IncludeSkippedTests) == true;

        return new(annotationTitleFormat, annotationMessageFormat, includePassedTests, includeSkippedTests);
    }
}
