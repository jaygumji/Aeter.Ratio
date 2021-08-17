/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class TypeValidArgs
    {
        public Type Type { get; }
        public bool IsValid { get; set; }

        public TypeValidArgs(Type type)
        {
            Type = type;
            IsValid = true;
        }

    }
}