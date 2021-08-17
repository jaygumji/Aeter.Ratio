/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aeter.Ratio.IoC
{
    public class IoCRegistrationConfigurator<T> : IIoCRegistrationConfigurator<T>, IIoCRegistrationSingletonConfigurator<T>
    {
        private readonly IIoCRegistrator _registrator;
        private readonly IoCRegistration<T> _registration;

        public IoCRegistrationConfigurator(IIoCRegistrator registrator, IoCRegistration<T> registration)
        {
            _registrator = registrator;
            _registration = registration;
        }

        IIoCRegistrationSingletonConfigurator<T> IIoCRegistrationSingletonConfigurator<T>.IncludeAllInterfaces()
        {
            IncludeAllInterfaces();
            return this;
        }

        IIoCRegistrationConfigurator<T> IIoCRegistrationConfigurator<T>.IncludeAllInterfaces()
        {
            IncludeAllInterfaces();
            return this;
        }

        private void IncludeAllInterfaces()
        {
            foreach (var type in _registration.Type.GetTypeInfo().GetInterfaces()) {
                _registrator.TryRegister(type, _registration);
            }
        }

        IIoCRegistrationSingletonConfigurator<T> IIoCRegistrationSingletonConfigurator<T>.IncludeAllBaseTypes()
        {
            IncludeAllBaseTypes();
            return this;
        }

        IIoCRegistrationConfigurator<T> IIoCRegistrationConfigurator<T>.IncludeAllBaseTypes()
        {
            IncludeAllBaseTypes();
            return this;
        }

        private void IncludeAllBaseTypes()
        {
            var info = _registration.Type.GetTypeInfo();
            while (info.BaseType != null) {
                _registrator.TryRegister(info.BaseType, _registration);
                info = info.BaseType.GetTypeInfo();
            }
        }

        IIoCRegistrationConfigurator<T> IIoCRegistrationConfigurator<T>.OnUnload(Action<T> unloader)
        {
            _registration.Unloader = unloader;
            return this;
        }

        IIoCRegistrationConfigurator<T> IIoCRegistrationConfigurator<T>.NeverScope()
        {
            if (_registration.MustBeScoped) {
                throw new InvalidOperationException($"The registration of {typeof(T).FullName} is marked as must be in scope.");
            }
            _registration.CanBeScoped = false;
            return this;
        }

        IIoCRegistrationConfigurator<T> IIoCRegistrationConfigurator<T>.MustBeScoped()
        {
            if (!_registration.CanBeScoped) {
                throw new InvalidOperationException($"The registration of {typeof(T).FullName} can not be used in a scope.");
            }
            _registration.MustBeScoped = true;
            return this;
        }

    }
}