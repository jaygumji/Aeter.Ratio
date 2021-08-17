/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Serialization.Reflection;
using System;

namespace Aeter.Ratio.Serialization
{
    public class VisitArgsTypeFactory : IVisitArgsTypeFactory
    {
        protected SerializableTypeProvider Provider { get; }

        public VisitArgsTypeFactory(SerializableTypeProvider provider)
        {
            Provider = provider;
        }

        public virtual IVisitArgsFactory ConstructWith(Type type)
        {
            return new VisitArgsFactory(Provider, type);
        }
    }
}