// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform;

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

// https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
internal sealed class GitHubWorkflow(TextWriter commandWriter, TextWriter summaryWriter)
{
    private void InvokeCommand(
        string command,
        string message,
        IReadOnlyDictionary<string, string> options)
    {
        // URL-encode certain characters to ensure they don't get parsed as command tokens
        // https://pakstech.com/blog/github-actions-workflow-commands
        static string Escape(string value) =>
            value
                .Replace("%", "%25")
                .Replace("\n", "%0A")
                .Replace("\r", "%0D");

        string? formattedOptions = string.Join(",", options.Select(kvp => Escape(kvp.Key) + '=' + Escape(kvp.Value)));

        // Command should start at the beginning of the line, so add a newline
        // to make sure there is no preceding text.
        // Preceding text may sometimes appear if the .NET CLI is running with
        // ANSI color codes enabled.
        commandWriter.WriteLine();

        commandWriter.WriteLine($"::{command} {formattedOptions}::{Escape(message)}");

        // This newline is just for symmetry
        commandWriter.WriteLine();

        commandWriter.Flush();
    }

    public void CreateErrorAnnotation(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null)
    {
        var options = new Dictionary<string, string> { ["title"] = title };

        if (!RoslynString.IsNullOrWhiteSpace(filePath))
        {
            options["file"] = filePath;
        }

        if (line is not null)
        {
            options["line"] = line.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (column is not null)
        {
            options["col"] = column.Value.ToString(CultureInfo.InvariantCulture);
        }

        InvokeCommand("error", message, options);
    }

    public void CreateSummary(string content)
    {
        // Other steps may have reported summaries that contain HTML tags,
        // which can screw up markdown parsing, so we need to make sure
        // there's at least two newlines before our summary to be safe.
        // https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/22
        summaryWriter.WriteLine();
        summaryWriter.WriteLine();

        summaryWriter.WriteLine(content);
        summaryWriter.Flush();
    }

    public static GitHubWorkflow Default { get; } =
        new(
            // Commands are written to the standard output
            Console.Out,
            // Summary is written to the file specified by an environment variable.
            // We may need to write to the summary file from multiple test suites in parallel,
            // so we should use a stream that delays acquiring the file lock until the very last moment,
            // and employs retry logic to handle potential race conditions.
            new StreamWriter(new ContentionTolerantWriteFileStream(Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY")!, FileMode.Append)));

    public static string? TryGenerateFilePermalink(string? filePath, int? line = null)
    {
        if (filePath is null)
        {
            return null;
        }

        string? serverUrl = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        string? repositorySlug = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        string? workspacePath = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        string? commitHash = Environment.GetEnvironmentVariable("GITHUB_SHA");

        if (RoslynString.IsNullOrWhiteSpace(serverUrl) ||
            RoslynString.IsNullOrWhiteSpace(repositorySlug) ||
            RoslynString.IsNullOrWhiteSpace(workspacePath) ||
            RoslynString.IsNullOrWhiteSpace(commitHash))
        {
            return null;
        }

        string filePathRelative =
            // If the file path starts with /_/ but the workspace path doesn't,
            // then it's safe to assume that the file path has already been normalized
            // by the Deterministic Build feature of MSBuild.
            // In this case, we only need to remove the leading /_/ from the file path
            // to get the correct relative path.
            filePath.StartsWith("/_/", StringComparison.Ordinal)
            && !workspacePath.StartsWith("/_/", StringComparison.Ordinal)
                ? filePath[3..]
                : PathEx.GetRelativePath(workspacePath, filePath);

        string filePathRoute = filePathRelative.Replace('\\', '/').Trim('/');
        string lineMarker = $"#L{line}";

        return $"{serverUrl}/{repositorySlug}/blob/{commitHash}/{filePathRoute}{lineMarker}";
    }
}
