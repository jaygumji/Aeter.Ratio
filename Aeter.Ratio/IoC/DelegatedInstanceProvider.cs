/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IoC
{
    public class DelegatedInstanceProvider<TInstance> : IInstanceFactory<TInstance>
    {
        private readonly IInstanceFactory _delegated;

        public DelegatedInstanceProvider(IInstanceFactory delegated)
        {
            _delegated = delegated;
        }

        public TInstance GetInstance()
        {
            return (TInstance)_delegated.GetInstance();
        }

        object IInstanceFactory.GetInstance()
        {
            return _delegated.GetInstance();
        }
    }
}