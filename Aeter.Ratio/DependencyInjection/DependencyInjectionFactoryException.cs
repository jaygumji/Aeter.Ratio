/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.DependencyInjection
{
    public class DependencyInjectionFactoryException : CodedException
    {

        public DependencyInjectionFactoryException(string message, int errorCode) : base(message, errorCode)
        {

        }

        public static Exception AmbigiousConstructor(Type type)
        {
            var msg = $"Unabled to find suitable constructor for {type.FullName}";
            return new DependencyInjectionFactoryException(msg, DependencyInjectionAmbigiousConstructor);
        }
    }
}