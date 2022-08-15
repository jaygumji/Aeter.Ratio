/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.Test.Fakes.DependencyInjection
{
    public class DependencyInjectionRegistratorMock : IDependencyInjectionRegistrator
    {
        public bool Contains(Type type)
        {
            return false;
        }

        public IDependencyInjectionRegistration Get(Type type)
        {
            throw new NotImplementedException();
        }

        public void Register(IDependencyInjectionRegistration registration)
        {
            throw new NotImplementedException();
        }

        public void Register(Type type, IDependencyInjectionRegistration registration)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(Type type, [NotNullWhen(true)] out IDependencyInjectionRegistration? registration)
        {
            registration = null;
            return false;
        }

        public bool TryRegister(Type type, IDependencyInjectionRegistration registration)
        {
            throw new NotImplementedException();
        }
    }
}
