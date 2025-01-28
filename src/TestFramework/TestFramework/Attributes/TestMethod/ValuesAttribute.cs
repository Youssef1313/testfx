// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// This attribute is used to provide values to a parameter of a test method.
/// </summary>
/// <remarks>
/// If the attribute is present on one parameter of a test method, it should be present for all parameters.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ValuesAttribute : Attribute
{
    public ValuesAttribute(params object[] values)
        => Values = values;

    public object[] Values { get; }
}
