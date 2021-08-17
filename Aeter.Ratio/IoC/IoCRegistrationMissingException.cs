/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IoC
{
    internal class IoCRegistrationNotFoundException : Exception
    {
        public IoCRegistrationNotFoundException(Type type) : base("No registration has been made for " + type.FullName)
        {
        }

        public IoCRegistrationNotFoundException(string message) : base(message)
        {
        }

        public IoCRegistrationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}