// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Services;

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

public static class GitHubReportExtensions
{
    /// <summary>
    /// Adds GitHub report support to the Testing Platform Builder.
    /// </summary>
    /// <param name="testApplicationBuilder">The test application builder.</param>
    public static void AddGitHubReportProvider(this ITestApplicationBuilder testApplicationBuilder)
    {
        var extension = new GitHubTestReporterExtension();

        var compositeExtension = new CompositeExtensionFactory<GitHubTestReporter>(serviceProvider =>
            new GitHubTestReporter(extension, serviceProvider.GetCommandLineOptions()));
        testApplicationBuilder.TestHost.AddDataConsumer(compositeExtension);
        testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(compositeExtension);

        testApplicationBuilder.CommandLine.AddProvider(() => new CliOptionsProvider(extension));
    }
}
