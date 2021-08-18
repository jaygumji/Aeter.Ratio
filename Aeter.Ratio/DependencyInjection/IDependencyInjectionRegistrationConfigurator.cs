/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.DependencyInjection
{
    public interface IDependencyInjectionRegistrationConfigurator<T>
    {
        IDependencyInjectionRegistrationConfigurator<T> IncludeAllInterfaces();
        IDependencyInjectionRegistrationConfigurator<T> IncludeAllBaseTypes();

        IDependencyInjectionRegistrationConfigurator<T> OnUnload(Action<T> unloader);
        IDependencyInjectionRegistrationConfigurator<T> NeverScope();
        IDependencyInjectionRegistrationConfigurator<T> MustBeScoped();
    }

}
