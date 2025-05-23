﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Test Owner.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OwnerAttribute : TestPropertyAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerAttribute"/> class.
    /// </summary>
    /// <param name="owner">
    /// The owner.
    /// </param>
    public OwnerAttribute(string? owner)
        : base("Owner", owner ?? string.Empty)
    {
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public string? Owner => Value;
}
