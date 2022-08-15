/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionScopedInstance
    {
        public IDependencyInjectionRegistration? Registration { get; }
        public object Instance { get; }

        public DependencyInjectionScopedInstance(IDependencyInjectionRegistration? registration, object instance)
        {
            Registration = registration;
            Instance = instance;
        }
    }
}