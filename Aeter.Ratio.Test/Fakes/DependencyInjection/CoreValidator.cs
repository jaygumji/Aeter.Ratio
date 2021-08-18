/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Xunit;

namespace Aeter.Ratio.Test.DependencyInjection.Fakes
{
    public class CoreValidator : ICoreValidator
    {
        private readonly int _expected;

        public CoreValidator(int expected)
        {
            _expected = expected;
        }

        public void Validate(int value)
        {
            Assert.Equal(_expected, value);
        }

    }
}