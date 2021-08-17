/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Threading;

namespace Aeter.Ratio.IoC
{
    public class IoCScope : IDisposable
    {

        private static readonly AsyncLocal<IoCScope> ScopeLocal = new AsyncLocal<IoCScope>();
        private Dictionary<Type, IoCScopedInstance> _instances;

        public IoCScope()
        {
            _instances = new Dictionary<Type, IoCScopedInstance>();

            Parent = ScopeLocal.Value;
            ScopeLocal.Value = this;
        }

        public IoCContainer Container { get; }
        public IoCScope Parent { get; private set; }

        public int InstanceCount => _instances.Count;

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
            IoCScope childScope = null;
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
                ScopeLocal.Value = storedScope.Parent;
            }
        }

        public void Register(IIoCRegistration registration, object instance)
        {
            _instances.Add(registration.Type, new IoCScopedInstance(registration, instance));
        }

        public void Register(object instance)
        {
            if (instance == null) return;
            var type = instance.GetType();
            _instances.Add(type, new IoCScopedInstance(null, instance));
        }

        public bool TryGetInstance(Type type, out object instance)
        {
            if (_instances.TryGetValue(type, out IoCScopedInstance instReg)) {
                instance = instReg.Instance;
                return true;
            }
            instance = null;
            return false;
        }

        public static IoCScope GetCurrent()
        {
            return ScopeLocal.Value;
        }
    }
}