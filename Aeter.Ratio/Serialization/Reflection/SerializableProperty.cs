/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class SerializableProperty
    {
        public SerializableProperty(PropertyInfo @ref, SerializationMetadata metadata, ITypeProvider provider)
        {
            Ref = @ref;
            Metadata = metadata;
            Ext = provider.Extend(Ref.PropertyType);
        }

        public PropertyInfo Ref { get; }

        public SerializationMetadata Metadata { get; }

        public ExtendedType Ext { get; }

    }
}