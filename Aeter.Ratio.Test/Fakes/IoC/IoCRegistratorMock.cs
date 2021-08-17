/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.IoC;

namespace Aeter.Ratio.Test.Fakes.IoC
{
    public class IoCRegistratorMock : IIoCRegistrator
    {
        public bool Contains(Type type)
        {
            return false;
        }

        public IIoCRegistration Get(Type type)
        {
            throw new NotImplementedException();
        }

        public void Register(IIoCRegistration registration)
        {
            throw new NotImplementedException();
        }

        public void Register(Type type, IIoCRegistration registration)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(Type type, out IIoCRegistration registration)
        {
            registration = null;
            return false;
        }

        public bool TryRegister(Type type, IIoCRegistration registration)
        {
            throw new NotImplementedException();
        }
    }
}
