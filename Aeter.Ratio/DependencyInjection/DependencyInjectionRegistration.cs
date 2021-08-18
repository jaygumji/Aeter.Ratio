/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionRegistration<T> : IDependencyInjectionRegistration<T>
    {
        public Type Type { get; }
        public Func<T> Factory { get; }

        public DependencyInjectionRegistration() : this(null)
        {
        }

        public DependencyInjectionRegistration(Func<T> factory)
        {
            Type = typeof(T);
            Factory = factory;
        }

        public bool CanBeScoped { get; set; }
        public Action<T> Unloader { get; set; }

        public bool HasInstanceGetter => Factory != null;

        public bool MustBeScoped { get; set; }

        public object GetInstance()
        {
            return Factory.Invoke();
        }

        T IInstanceFactory<T>.GetInstance()
        {
            return Factory.Invoke();
        }

        void IDependencyInjectionRegistration.Unload(object instance)
        {
            if (Unloader != null) {
                var typedInstance = (T)instance;
                Unloader.Invoke(typedInstance);
            }

            (instance as IDisposable)?.Dispose();
        }

    }
}