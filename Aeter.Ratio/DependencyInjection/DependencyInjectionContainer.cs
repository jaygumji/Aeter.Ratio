/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionContainer : IDependencyInjectionRegistrator, Aeter.Ratio.IInstanceFactory
    {

        private readonly Dictionary<Type, IDependencyInjectionRegistration> _registrations;
        private readonly DependencyInjectionFactory _factory;
        private readonly IDependencyInjectionRegistrator _registrator;

        public DependencyInjectionContainer() : this(new FactoryTypeProvider())
        {
        }

        public DependencyInjectionContainer(ITypeProvider provider)
        {
            _registrator = this;
            _registrations = new Dictionary<Type, IDependencyInjectionRegistration>();
            _factory = new DependencyInjectionFactory(_registrator, provider);
        }

        bool IDependencyInjectionRegistrator.TryGet(Type type, [MaybeNullWhen(false)] out IDependencyInjectionRegistration registration)
        {
            return _registrations.TryGetValue(type, out registration);
        }

        void IDependencyInjectionRegistrator.Register(IDependencyInjectionRegistration registration)
        {
            _registrator.Register(registration.Type, registration);
        }

        void IDependencyInjectionRegistrator.Register(Type type, IDependencyInjectionRegistration registration)
        {
            if (_registrations.ContainsKey(type)) {
                _registrations[type] = registration;
            }
            else {
                _registrations.Add(type, registration);
            }
        }

        bool IDependencyInjectionRegistrator.Contains(Type type)
        {
            return _registrations.ContainsKey(type);
        }

        IDependencyInjectionRegistration IDependencyInjectionRegistrator.Get(Type type)
        {
            if (_registrations.TryGetValue(type, out var registration)) {
                return registration;
            }

            throw new DependencyInjectionRegistrationNotFoundException(type);
        }


        bool IDependencyInjectionRegistrator.TryRegister(Type type, IDependencyInjectionRegistration registration)
        {
            if (_registrations.ContainsKey(type)) return false;

            _registrations.Add(type, registration);
            return true;
        }

        public IDependencyInjectionRegistrationSingletonConfigurator<T> Register<T>(T singleton)
            where T : notnull
        {
            var registration = new DependencyInjectionRegistration<T>(() => singleton) {
                CanBeScoped = false
            };

            _registrator.Register(registration);
            return new DependencyInjectionRegistrationConfigurator<T>(_registrator, registration);
        }

        public IDependencyInjectionRegistrationConfigurator<TImplementation> Register<T, TImplementation>()
            where TImplementation : notnull
        {
            var registration = new DependencyInjectionRegistration<TImplementation>() {
                CanBeScoped = true
            };
            _registrator.Register(typeof(T), registration);
            _registrator.Register(registration);
            return new DependencyInjectionRegistrationConfigurator<TImplementation>(_registrator, registration);
        }

        public IDependencyInjectionRegistrationConfigurator<T> Register<T>(Func<T> factory)
        {
            var registration = new DependencyInjectionRegistration<T>(factory) {
                CanBeScoped = true
            };
            _registrator.Register(registration);
            return new DependencyInjectionRegistrationConfigurator<T>(_registrator, registration);
        }

        public T GetInstance<T>()
        {
            return (T)GetInstance(typeof(T), throwError: true)!;
        }

        public object? GetInstance(Type type)
        {
            return GetInstance(type, throwError: true);
        }

        public bool TryGetInstance(Type type, [MaybeNullWhen(false)] out object instance)
        {
            instance = GetInstance(type, throwError: false);
            return instance != null;
        }

        private object? GetInstance(Type type, bool throwError)
        {
            var scope = DependencyInjectionScope.GetCurrent();
            var hasScope = scope != null;
            if (hasScope && scope!.TryGetInstance(type, out var instance)) {
                return instance;
            }

            var hasRegistration = _registrator.TryGet(type, out var registration);
            if (hasRegistration && registration!.MustBeScoped && !hasScope) {
                if (throwError) {
                    throw new InvalidOperationException($"The type {type.FullName} could not be created since it requires a scope");
                }
                else {
                    return null;
                }
            }

            var newInstance = _factory.GetInstance(type, throwError);
            if (newInstance == null) {
                return null;
            }
            if (hasScope) {
                if (hasRegistration) {
                    if (!registration!.CanBeScoped) return newInstance;
                    scope!.Register(registration, newInstance);
                }
                else {
                    scope!.Register(newInstance);
                }
            }
            return newInstance;
        }

    }
}
