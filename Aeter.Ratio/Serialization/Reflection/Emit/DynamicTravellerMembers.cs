/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public sealed class DynamicTravellerMembers
    {

        public readonly Type VisitArgsType;
        public readonly Type VisitArgsFactoryType;
        public readonly Type VisitArgsTypeFactoryType;
        public readonly MethodInfo ConstructVisitArgsMethod;
        public readonly MethodInfo ConstructVisitArgsWithTypeMethod;

        public readonly Type[] TravellerConstructorTypes;

        public DynamicTravellerMembers()
        {
            VisitArgsType = typeof(VisitArgs);
            VisitArgsFactoryType = typeof(IVisitArgsFactory);
            VisitArgsTypeFactoryType = typeof(IVisitArgsTypeFactory);

            ConstructVisitArgsMethod = VisitArgsFactoryType.FindMethod("Construct");
            ConstructVisitArgsWithTypeMethod = VisitArgsTypeFactoryType.FindMethod("ConstructWith");

            TravellerConstructorTypes = new[] { VisitArgsFactoryType };
        }

    }
}
