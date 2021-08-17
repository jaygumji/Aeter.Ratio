/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class BuildTravellerMethodArgs
    {
        private readonly IReadOnlyDictionary<Type, PendingChildGraphTraveller> _childTravellers;
        private readonly IReadOnlyDictionary<SerializableProperty, FieldInfo> _argFields;

        public BuildTravellerMethodArgs(IReadOnlyDictionary<Type, PendingChildGraphTraveller> childTravellers, IReadOnlyDictionary<SerializableProperty, FieldInfo> argFields)
        {
            _childTravellers = childTravellers;
            _argFields = argFields;
        }

        public PendingChildGraphTraveller GetTraveller(Type type)
        {
            if (!_childTravellers.TryGetValue(type, out var childTravellerInfo))
                throw InvalidGraphException.ComplexTypeWithoutTravellerDefined(type);

            return childTravellerInfo;
        }

        public FieldInfo GetArgsField(SerializableProperty ser)
        {
            if (!_argFields.TryGetValue(ser, out var field))
                throw new ArgumentException("Could not find the visit args field for the property " + ser.Ref.Name);

            return field;
        }
    }
}