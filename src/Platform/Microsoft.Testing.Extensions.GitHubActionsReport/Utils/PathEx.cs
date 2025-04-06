// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal static class PathEx
{
    private static readonly StringComparison PathStringComparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    // This method exists on .NET 5+ but it's impossible to polyfill static
    // members, so we'll just use this one on all targets.
    public static string GetRelativePath(string basePath, string path)
    {
        string[] basePathSegments = basePath.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        string[] pathSegments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        int commonSegmentsCount = 0;
        for (int i = 0; i < basePathSegments.Length && i < pathSegments.Length; i++)
        {
            if (!string.Equals(basePathSegments[i], pathSegments[i], PathStringComparison))
            {
                break;
            }

            commonSegmentsCount++;
        }

        return string.Join(
            Path.DirectorySeparatorChar.ToString(),
            pathSegments.Skip(commonSegmentsCount));
    }
}
