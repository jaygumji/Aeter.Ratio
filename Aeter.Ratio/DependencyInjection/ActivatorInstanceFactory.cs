/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Reflection;

namespace Aeter.Ratio.DependencyInjection
{
    public class ActivatorInstanceFactory<TInstance> : IInstanceFactory<TInstance>
    {
        private readonly ConstructorInfo _constructor;
        private readonly IInstanceFactory[] _factories;

        public ActivatorInstanceFactory(ConstructorInfo constructor, params IInstanceFactory[] factories)
        {
            _constructor = constructor;
            _factories = factories;
        }

        public TInstance GetInstance()
        {
            return (TInstance) _constructor.Invoke(_factories.Select(f => f.GetInstance()).ToArray());
        }

        object IInstanceFactory.GetInstance()
        {
            return GetInstance();
        }
    }
}