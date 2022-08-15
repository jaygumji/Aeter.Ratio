/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio
{
    public interface IInstanceFactory
    {
        object? GetInstance(Type type);
        bool TryGetInstance(Type type, [MaybeNullWhen(false)] out object instance);
    }
}
