// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal static class TimeSpanExtensions
{
    public static string ToHumanString(this TimeSpan timeSpan) =>
        timeSpan switch
        {
            { TotalSeconds: <= 1 } => timeSpan.Milliseconds + "ms",
            { TotalMinutes: <= 1 } => timeSpan.Seconds + "s",
            { TotalHours: <= 1 } => timeSpan.Minutes + "m" + timeSpan.Seconds + "s",
            _ => timeSpan.Hours + "h" + timeSpan.Minutes + "m",
        };
}
