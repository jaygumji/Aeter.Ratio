/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Test.DependencyInjection.Fakes
{
    public class CoreConfig
    {
        public int Delta { get; set; }

        public static CoreConfig Default()
        {
            return new CoreConfig { Delta = 2 };
        }
    }
}