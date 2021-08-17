/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using System.Reflection;

namespace Aeter.Ratio.Reflection
{
    class PropertyAccessor : IPropertyAccessor
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyAccessor(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public IEnumerable<object> GetValuesOf(IEnumerable<object> values)
        {
            var next = new List<object>();

            foreach (var value in values) {
                var nextValue = _propertyInfo.GetValue(value);
                if (nextValue == null) continue;
                next.Add(nextValue);
            }

            return next;
        }
    }
}