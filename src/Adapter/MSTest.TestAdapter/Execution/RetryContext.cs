// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.ObjectModel;

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

internal sealed class RetryContext
{
    internal RetryContext(Func<Task<UnitTestResult[]>> executeTaskGetter)
        => ExecuteTaskGetter = executeTaskGetter;

    public Func<Task<UnitTestResult[]>> ExecuteTaskGetter { get; }
}
