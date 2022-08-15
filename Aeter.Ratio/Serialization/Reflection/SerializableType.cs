/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class SerializableType
    {
        private readonly Type _type;
        private readonly IReadOnlyDictionary<string, SerializableProperty> _properties;

        public SerializableType(Type type, IReadOnlyDictionary<string, SerializableProperty> properties)
        {
            _type = type;
            _properties = properties;
        }

        public Type Type { get { return _type; } }

        public IEnumerable<SerializableProperty> Properties
        {
            get { return _properties.Values; }
        }

        public SerializableProperty FindProperty(string propertyName)
        {
            if (!_properties.TryGetValue(propertyName, out var ser))
                throw new ArgumentException("Property was not found, " + propertyName);

            return ser;
        }
    }
}
