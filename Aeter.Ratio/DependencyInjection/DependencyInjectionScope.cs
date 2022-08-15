/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionScope : IDisposable
    {

        private static readonly AsyncLocal<DependencyInjectionScope> ScopeLocal = new AsyncLocal<DependencyInjectionScope>();
        private Dictionary<Type, DependencyInjectionScopedInstance>? _instances;

        public DependencyInjectionScope()
        {
            _instances = new Dictionary<Type, DependencyInjectionScopedInstance>();

            Parent = ScopeLocal.Value;
            ScopeLocal.Value = this;
        }

        public DependencyInjectionScope? Parent { get; private set; }

        public int InstanceCount => _instances?.Count ?? 0;

        public void Dispose()
        {
            if (_instances != null) {
                foreach (var instance in _instances.Values) {
                    if (instance.Registration != null) {
                        instance.Registration.Unload(instance.Instance);
                    }
                    else {
                        (instance.Instance as IDisposable)?.Dispose();
                    }
                }

                _instances = null;
            }

            var storedScope = ScopeLocal.Value;
            DependencyInjectionScope? childScope = null;
            while (storedScope != null && !ReferenceEquals(storedScope, this)) {
                childScope = storedScope;
                storedScope = storedScope.Parent;
            }

            if (storedScope == null) {
                throw new ArgumentException("The scope could not be found in the scope chain");
            }

            if (childScope != null) {
                childScope.Parent = storedScope.Parent;
            }
            else {
                ScopeLocal.Value = storedScope.Parent!;
            }
        }

        public void Register(IDependencyInjectionRegistration registration, object instance)
        {
            if (_instances == null) throw new InvalidOperationException("Scope is disposed");
            _instances.Add(registration.Type, new DependencyInjectionScopedInstance(registration, instance));
        }

        public void Register(object instance)
        {
            if (_instances == null) throw new InvalidOperationException("Scope is disposed");
            if (instance == null) return;
            var type = instance.GetType();
            _instances.Add(type, new DependencyInjectionScopedInstance(null, instance));
        }

        public bool TryGetInstance(Type type, [MaybeNullWhen(false)] out object instance)
        {
            if (_instances == null) throw new InvalidOperationException("Scope is disposed");
            if (_instances.TryGetValue(type, out var instReg)) {
                instance = instReg.Instance;
                return true;
            }
            instance = null;
            return false;
        }

        public static DependencyInjectionScope? GetCurrent()
        {
            return ScopeLocal.Value;
        }
    }
}