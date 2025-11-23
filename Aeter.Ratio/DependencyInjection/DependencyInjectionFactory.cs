/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionFactory
    {
        private readonly IDependencyInjectionRegistrator _registrator;
        private readonly ITypeProvider _provider;
        private readonly IDictionary<Type, IInstanceFactory> _typeFactories;

        public DependencyInjectionFactory(IDependencyInjectionRegistrator registrator, ITypeProvider provider)
        {
            _registrator = registrator;
            _provider = provider;
            _typeFactories = new Dictionary<Type, IInstanceFactory>();
        }

        public IInstanceFactory? GetFactory(Type type, bool throwError)
        {
            if (_typeFactories.TryGetValue(type, out var factory)) {
                return factory;
            }

            if (_registrator.TryGet(type, out var registration)) {
                if (registration.HasInstanceGetter) {
                    if (type == registration.Type) {
                        _typeFactories.Add(type, registration);
                        return registration;
                    }
                    factory = (IInstanceFactory)Activator.CreateInstance(typeof(DelegatedInstanceProvider<>).MakeGenericType(type), registration)!;
                    _typeFactories.Add(type, factory);
                    return factory;
                }
                else if (type != registration.Type) {
                    factory = GetFactory(registration.Type, throwError);
                    if (factory != null) {
                        factory = (IInstanceFactory)Activator.CreateInstance(typeof(DelegatedInstanceProvider<>).MakeGenericType(type), factory)!;
                        _typeFactories.Add(type, factory);
                    }
                    return factory;
                }
            }

            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (constructors.Length > 1) {
                constructors = constructors.Where(c => c.IsPublic).ToArray();
            }

            if (constructors.Length != 1) {
                if (throwError) {
                    throw DependencyInjectionFactoryException.AmbigiousConstructor(type);
                }
                else {
                    return null;
                }
            }

            var constructor = constructors[0];

            var parameters = constructor.GetParameters();
            var paramLength = parameters.Length;

            var properties = info.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite && _registrator.Contains(p.PropertyType))
                .ToArray();

            paramLength += properties.Length;

            var factoryParams = new object[paramLength > 0 ? paramLength + 2 : 1];
            var factoryTypes = new Type[paramLength + 1];
            factoryTypes[0] = type;
            factoryParams[0] = constructor;

            if (paramLength > 0) {
                factoryParams[1] = properties;
            }

            for (var i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                var paramFactory = GetFactory(parameter.ParameterType, throwError);
                if (paramFactory == null) return null;
                factoryParams[i + 2] = paramFactory;
                factoryTypes[i + 1] = parameter.ParameterType;
            }

            if (properties.Length > 0) {
                var paramIndex = parameters.Length + 1;
                for (var i = 0; i < properties.Length; i++) {
                    var propertyType = properties[i].PropertyType;
                    factoryParams[paramIndex + 1] = GetFactory(propertyType, throwError)!;
                    factoryTypes[paramIndex++] = propertyType;
                }
            }

            var factoryType = GetFactoryType(factoryTypes, out bool useDynamic);
            var factoryConstructor = factoryType.GetConstructors()[0];

            if (useDynamic) {
                factory = (IInstanceFactory)factoryConstructor.Invoke(new object[] {
                    constructor,
                    properties,
                    _provider,
                    factoryParams.OfType<IInstanceFactory>().ToArray()
                });
            }
            else {
                factory = (IInstanceFactory)factoryConstructor.Invoke(factoryParams);
            }
            _typeFactories.Add(type, factory);

            return factory;
        }

        private Type GetFactoryType(Type[] types, out bool useDynamic)
        {
            useDynamic = false;
            switch (types.Length) {
                case 1:
                    return typeof(InstanceFactory<>).MakeGenericType(types);
                case 2:
                    return typeof(InstanceFactory<,>).MakeGenericType(types);
                case 3:
                    return typeof(InstanceFactory<,,>).MakeGenericType(types);
                case 4:
                    return typeof(InstanceFactory<,,,>).MakeGenericType(types);
                case 5:
                    return typeof(InstanceFactory<,,,,>).MakeGenericType(types);
                case 6:
                    return typeof(InstanceFactory<,,,,,>).MakeGenericType(types);
                case 7:
                    return typeof(InstanceFactory<,,,,,,>).MakeGenericType(types);
                case 8:
                    return typeof(InstanceFactory<,,,,,,,>).MakeGenericType(types);
                case 9:
                    return typeof(InstanceFactory<,,,,,,,,>).MakeGenericType(types);
                case 10:
                    return typeof(InstanceFactory<,,,,,,,,,>).MakeGenericType(types);
                case 11:
                    return typeof(InstanceFactory<,,,,,,,,,,>).MakeGenericType(types);
                case 12:
                    return typeof(InstanceFactory<,,,,,,,,,,,>).MakeGenericType(types);
                case 13:
                    return typeof(InstanceFactory<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14:
                    return typeof(InstanceFactory<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15:
                    return typeof(InstanceFactory<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16:
                    return typeof(InstanceFactory<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17:
                    return typeof(InstanceFactory<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
            }
            useDynamic = true;
            return typeof(DynamicActivatorInstanceFactory<>).MakeGenericType(types[0]);
        }

        public object? GetInstance(Type type, bool throwError)
        {
            var factory = GetFactory(type, throwError);
            if (factory != null) return factory.GetInstance();

            if (throwError) {
                throw new ArgumentException("Error when attempting to retrieve factory.");
            }
            return null;
        }

    }
}
