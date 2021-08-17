/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.IoC
{
    public class IoCScopedInstance
    {
        public IIoCRegistration Registration { get; }
        public object Instance { get; }

        public IoCScopedInstance(IIoCRegistration registration, object instance)
        {
            Registration = registration;
            Instance = instance;
        }
    }
}