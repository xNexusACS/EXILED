﻿// -----------------------------------------------------------------------
// <copyright file="NonNegativeAttribute.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Attributes.Validators
{
    using System;

    using Exiled.API.Interfaces;

    /// <summary>
    /// An attribute that validates if the value of the marked property is non-negative.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NonNegativeAttribute : Attribute, IValidator
    {
        /// <inheritdoc/>
        public bool Validate(object value) => value is >= 0;
    }
}