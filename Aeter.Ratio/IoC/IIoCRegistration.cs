/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IoC
{
    public interface IIoCRegistration : IInstanceFactory
    {
        Type Type { get; }
        bool CanBeScoped { get; }
        bool MustBeScoped { get; }
        bool HasInstanceGetter { get; }

        void Unload(object instance);
    }

    public interface IIoCRegistration<T> : IIoCRegistration, IInstanceFactory<T>
    {

    }
}