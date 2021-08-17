/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IoC
{
    public interface IIoCRegistrator
    {
        void Register(IIoCRegistration registration);
        void Register(Type type, IIoCRegistration registration);
        bool TryRegister(Type type, IIoCRegistration registration);

        IIoCRegistration Get(Type type);
        bool TryGet(Type type, out IIoCRegistration registration);

        bool Contains(Type type);
    }
}