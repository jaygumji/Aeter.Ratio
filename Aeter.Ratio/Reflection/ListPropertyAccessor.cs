/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aeter.Ratio.Reflection
{
    class ListPropertyAccessor : IPropertyAccessor
    {
        private readonly PropertyInfo _propertyInfo;

        public ListPropertyAccessor(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public IEnumerable<object> GetValuesOf(IEnumerable<object> values)
        {
            var next = new List<object>();

            foreach (var value in values) {
                var list = (IList) _propertyInfo.GetValue(value);
                if (list == null) continue;
                foreach (var element in list) next.Add(element);
            }

            return next;
        }
    }
}