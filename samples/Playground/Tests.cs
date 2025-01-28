﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 0)]

namespace Playground;

[TestClass]
public class TestClass
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Test3([Values(0, 1, 2)] int a, [Values(3, 4, 5)] int b)
        => Assert.AreNotEqual(a, b);

    public static IEnumerable<(int A, int B)> Data
    {
        get
        {
            yield return (1, 2);
            yield return (3, 4);
        }
    }
}
