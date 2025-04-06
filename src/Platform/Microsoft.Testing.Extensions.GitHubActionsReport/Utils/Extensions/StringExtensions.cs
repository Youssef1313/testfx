// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal static class StringExtensions
{
    public static StringBuilder Trim(this StringBuilder builder)
    {
        while (builder.Length > 0 && char.IsWhiteSpace(builder[0]))
        {
            builder.Remove(0, 1);
        }

        while (builder.Length > 0 && char.IsWhiteSpace(builder[builder.Length - 1]))
        {
            builder.Remove(builder.Length - 1, 1);
        }

        return builder;
    }
}
