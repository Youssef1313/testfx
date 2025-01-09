// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// This attribute is used to set a retry count on a test method in case of failure.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class RetryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryAttribute"/> class with the given number of max retries.
    /// </summary>
    public RetryAttribute(int maxRetries)
        => MaxRetries = maxRetries;

    /// <summary>
    /// Gets the number of retries that the test should make in case of failures.
    /// </summary>
    public int MaxRetries { get; }
}
