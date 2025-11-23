/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionRegistrationConfigurator<T> : IDependencyInjectionRegistrationConfigurator<T>, IDependencyInjectionRegistrationSingletonConfigurator<T>
    {
        private readonly IDependencyInjectionRegistrator _registrator;
        private readonly DependencyInjectionRegistration<T> _registration;

        public DependencyInjectionRegistrationConfigurator(IDependencyInjectionRegistrator registrator, DependencyInjectionRegistration<T> registration)
        {
            _registrator = registrator;
            _registration = registration;
        }

        IDependencyInjectionRegistrationSingletonConfigurator<T> IDependencyInjectionRegistrationSingletonConfigurator<T>.IncludeAllInterfaces()
        {
            IncludeAllInterfaces();
            return this;
        }

        IDependencyInjectionRegistrationConfigurator<T> IDependencyInjectionRegistrationConfigurator<T>.IncludeAllInterfaces()
        {
            IncludeAllInterfaces();
            return this;
        }

        private void IncludeAllInterfaces()
        {
            foreach (var type in _registration.Type.GetInterfaces()) {
                _registrator.TryRegister(type, _registration);
            }
        }

        IDependencyInjectionRegistrationSingletonConfigurator<T> IDependencyInjectionRegistrationSingletonConfigurator<T>.IncludeAllBaseTypes()
        {
            IncludeAllBaseTypes();
            return this;
        }

        IDependencyInjectionRegistrationConfigurator<T> IDependencyInjectionRegistrationConfigurator<T>.IncludeAllBaseTypes()
        {
            IncludeAllBaseTypes();
            return this;
        }

        private void IncludeAllBaseTypes()
        {
            var current = _registration.Type;
            while (current.BaseType != null) {
                var baseType = current.BaseType;
                _registrator.TryRegister(baseType, _registration);
                current = baseType;
            }
        }

        IDependencyInjectionRegistrationConfigurator<T> IDependencyInjectionRegistrationConfigurator<T>.OnUnload(Action<T> unloader)
        {
            _registration.Unloader = unloader;
            return this;
        }

        IDependencyInjectionRegistrationConfigurator<T> IDependencyInjectionRegistrationConfigurator<T>.NeverScope()
        {
            if (_registration.MustBeScoped) {
                throw new InvalidOperationException($"The registration of {typeof(T).FullName} is marked as must be in scope.");
            }
            _registration.CanBeScoped = false;
            return this;
        }

        IDependencyInjectionRegistrationConfigurator<T> IDependencyInjectionRegistrationConfigurator<T>.MustBeScoped()
        {
            if (!_registration.CanBeScoped) {
                throw new InvalidOperationException($"The registration of {typeof(T).FullName} can not be used in a scope.");
            }
            _registration.MustBeScoped = true;
            return this;
        }

    }
}
