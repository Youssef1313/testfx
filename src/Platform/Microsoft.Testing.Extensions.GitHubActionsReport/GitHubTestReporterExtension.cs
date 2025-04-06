// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Helpers;

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal sealed class GitHubTestReporterExtension : IExtension
{
    public string Uid => nameof(GitHubTestReporterExtension);

    public string Version => AppVersion.DefaultSemVer;

    public string DisplayName => "GitHub test reporter";

    public string Description => "Reports test run information to GitHub Actions";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);
}
