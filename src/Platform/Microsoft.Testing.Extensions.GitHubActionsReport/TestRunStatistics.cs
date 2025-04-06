// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal sealed record TestRunStatistics(
    int PassedTestCount,
    int FailedTestCount,
    int SkippedTestCount,
    int TotalTestCount,
    TimeSpan OverallDuration);
