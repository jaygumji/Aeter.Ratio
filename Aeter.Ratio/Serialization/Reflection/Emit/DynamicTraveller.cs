/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class DynamicTraveller
    {
        private readonly DynamicTravellerMembers _members;
        private bool _isConstructing;
        private DynamicActivator? _activator;

        public DynamicTraveller(Type travellerType, ConstructorInfo constructor, MethodInfo travelWriteMethod, MethodInfo travelReadMethod, DynamicTravellerMembers members)
        {
            TravellerType = travellerType;
            Constructor = constructor;
            TravelWriteMethod = travelWriteMethod;
            TravelReadMethod = travelReadMethod;
            _members = members;
            _isConstructing = true;
        }

        public Type TravellerType { get; private set; }

        public ConstructorInfo Constructor { get; private set; }

        public MethodInfo TravelWriteMethod { get; private set; }

        public MethodInfo TravelReadMethod { get; private set; }

        public void Complete(Type actualTravellerType)
        {
            TravellerType = actualTravellerType;
            Constructor = actualTravellerType.FindConstructor(_members.TravellerConstructorTypes);
            TravelWriteMethod = actualTravellerType.FindMethod("Travel", TravelWriteMethod.GetParameters().Select(p => p.ParameterType).ToArray());
            TravelReadMethod = actualTravellerType.FindMethod("Travel", TravelReadMethod.GetParameters().Select(p => p.ParameterType).ToArray());
            _activator = new DynamicActivator(Constructor);
            _isConstructing = false;
        }

        public IGraphTraveller GetInstance(IVisitArgsFactory factory)
        {
            if (_isConstructing) throw new InvalidOperationException("The type is still being constructed");

            return (IGraphTraveller)_activator!.Activate(factory);
        }
    }
}