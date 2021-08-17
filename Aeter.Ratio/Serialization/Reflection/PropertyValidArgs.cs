/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class PropertyValidArgs
    {
        private readonly Type _type;
        private readonly PropertyInfo _property;

        public bool IsValid { get; set; }

        public PropertyValidArgs(Type type, PropertyInfo property)
        {
            _type = type;
            _property = property;
            IsValid = true;
        }

        public Type Type
        {
            get { return _type; }
        }

        public PropertyInfo Property
        {
            get { return _property; }
        }
    }
}