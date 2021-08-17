/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.IoC
{
    public class IoCContainer : IIoCRegistrator, Aeter.Ratio.IInstanceFactory
    {

        private readonly Dictionary<Type, IIoCRegistration> _registrations;
        private readonly IoCFactory _factory;
        private readonly IIoCRegistrator _registrator;

        public IoCContainer() : this(new FactoryTypeProvider())
        {
        }

        public IoCContainer(ITypeProvider provider)
        {
            _registrator = this;
            _registrations = new Dictionary<Type, IIoCRegistration>();
            _factory = new IoCFactory(_registrator, provider);
        }

        bool IIoCRegistrator.TryGet(Type type, out IIoCRegistration registration)
        {
            return _registrations.TryGetValue(type, out registration);
        }

        void IIoCRegistrator.Register(IIoCRegistration registration)
        {
            _registrator.Register(registration.Type, registration);
        }

        void IIoCRegistrator.Register(Type type, IIoCRegistration registration)
        {
            if (_registrations.ContainsKey(type))
            {
                _registrations[type] = registration;
            }
            else
            {
                _registrations.Add(type, registration);
            }
        }

        bool IIoCRegistrator.Contains(Type type)
        {
            return _registrations.ContainsKey(type);
        }

        IIoCRegistration IIoCRegistrator.Get(Type type)
        {
            if (_registrations.TryGetValue(type, out IIoCRegistration registration))
            {
                return registration;
            }

            throw new IoCRegistrationNotFoundException(type);
        }


        bool IIoCRegistrator.TryRegister(Type type, IIoCRegistration registration)
        {
            if (_registrations.ContainsKey(type)) return false;

            _registrations.Add(type, registration);
            return true;
        }

        public IIoCRegistrationSingletonConfigurator<T> Register<T>(T singleton)
        {
            var registration = new IoCRegistration<T>(() => singleton)
            {
                CanBeScoped = false
            };

            _registrator.Register(registration);
            return new IoCRegistrationConfigurator<T>(_registrator, registration);
        }

        public IIoCRegistrationConfigurator<TImplementation> Register<T, TImplementation>()
        {
            var registration = new IoCRegistration<TImplementation>()
            {
                CanBeScoped = true
            };
            _registrator.Register(typeof(T), registration);
            _registrator.Register(registration);
            return new IoCRegistrationConfigurator<TImplementation>(_registrator, registration);
        }

        public IIoCRegistrationConfigurator<T> Register<T>(Func<T> factory)
        {
            var registration = new IoCRegistration<T>(factory)
            {
                CanBeScoped = true
            };
            _registrator.Register(registration);
            return new IoCRegistrationConfigurator<T>(_registrator, registration);
        }

        public T GetInstance<T>()
        {
            return (T)GetInstance(typeof(T), throwError: true);
        }

        public object GetInstance(Type type)
        {
            return GetInstance(type, throwError: true);
        }

        public bool TryGetInstance(Type type, out object instance)
        {
            instance = GetInstance(type, throwError: false);
            return instance != null;
        }

        private object GetInstance(Type type, bool throwError)
        {
            var scope = IoCScope.GetCurrent();
            var hasScope = scope != null;
            if (hasScope && scope.TryGetInstance(type, out object instance))
            {
                return instance;
            }

            var hasRegistration = _registrator.TryGet(type, out IIoCRegistration registration);
            if (hasRegistration && registration.MustBeScoped && !hasScope)
            {
                if (throwError)
                {
                    throw new InvalidOperationException($"The type {type.FullName} could not be created since it requires a scope");
                }
                else
                {
                    return null;
                }
            }

            var newInstance = _factory.GetInstance(type, throwError);
            if (newInstance == null)
            {
                return null;
            }
            if (hasScope)
            {
                if (hasRegistration)
                {
                    if (!registration.CanBeScoped) return newInstance;
                    scope.Register(registration, newInstance);
                }
                else
                {
                    scope.Register(newInstance);
                }
            }
            return newInstance;
        }

    }
}
