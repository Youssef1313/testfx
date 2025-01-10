// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

public sealed class RetryResult
{
    private readonly List<TestResult[]> _testResults = new();

    public void AddResult(TestResult[] testResults)
        => _testResults.Add(testResults);

    internal TestResult[]? TryGetLast()
        => _testResults.Count > 0 ? _testResults[_testResults.Count - 1] : null;
}

/// <summary>
/// This attribute is used to set a retry count on a test method in case of failure.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class RetryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the given number of max retries.
    /// </summary>
    public RetryAttribute(int maxRetryAttempts)
        => MaxRetryAttempts = maxRetryAttempts;

    /// <summary>
    /// Gets the number of retries that the test should make in case of failures.
    /// </summary>
    public int MaxRetryAttempts { get; }

    public int MillisecondsDelayBetweenRetries { get; set; }

    /// <summary>
    /// Retries the test method <see cref="MaxRetryAttempts"/> times in case of failure.
    /// Note that a first run of the method was already executed and failed before this method is called.
    /// </summary>
    /// <param name="retryContext">An object to encapsulate the state needed for retry execution</param>
    /// <returns>
    /// Returns a <see cref="RetryResult"/> object that contains the results of all attempts. Only
    /// the last added element is used to determine the test outcome.
    /// The other results are currently not used, but may be used in the future for tooling to show the
    /// state of the failed attempts.
    /// </returns>
    protected internal virtual async Task<RetryResult> ExecuteAsync(RetryContext retryContext)
    {
        var result = new RetryResult();
        for (int i = 0; i < MaxRetryAttempts; i++)
        {
            // The caller already executed the test once. So we need to do the delay here.
            await Task.Delay(MillisecondsDelayBetweenRetries);

            TestResult[] testResults = await retryContext.ExecuteTaskGetter();
            result.AddResult(testResults);
            if (IsAcceptableResultForRetry(testResults))
            {
                break;
            }
        }

        return result;
    }

    internal static bool IsAcceptableResultForRetry(TestResult[] results)
    {
        foreach (TestResult result in results)
        {
            UnitTestOutcome outcome = result.Outcome;
            if (outcome is UnitTestOutcome.Failed or UnitTestOutcome.Timeout)
            {
                return false;
            }
        }

        return true;
    }
}
