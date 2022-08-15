/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.DependencyInjection
{
    public interface IDependencyInjectionRegistrator
    {
        void Register(IDependencyInjectionRegistration registration);
        void Register(Type type, IDependencyInjectionRegistration registration);
        bool TryRegister(Type type, IDependencyInjectionRegistration registration);

        IDependencyInjectionRegistration Get(Type type);
        bool TryGet(Type type, [MaybeNullWhen(false)] out IDependencyInjectionRegistration registration);

        bool Contains(Type type);
    }
}