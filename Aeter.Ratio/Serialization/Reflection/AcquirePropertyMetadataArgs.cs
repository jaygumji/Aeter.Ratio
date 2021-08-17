/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class AcquirePropertyMetadataArgs
    {
        public Type Type { get; }

        public PropertyInfo Property { get; }

        public uint? Index { get; set; }

        public AcquirePropertyMetadataArgs(Type type, PropertyInfo property)
        {
            Type = type;
            Property = property;
        }

    }
}