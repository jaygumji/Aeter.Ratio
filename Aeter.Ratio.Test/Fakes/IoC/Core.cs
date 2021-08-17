/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Test.Fakes.IoC;

namespace Aeter.Ratio.Test.IoC.Fakes
{
    public class Core : Command
    {
        private readonly CoreConfig _config;
        private readonly ICoreCalculator _calculator;
        private readonly ICoreValidator _validator;

        public Core(CoreConfig config, ICoreCalculator calculator, ICoreValidator validator)
        {
            _config = config;
            _calculator = calculator;
            _validator = validator;
        }

        public int Calculate(int x, int y, int z)
        {
            Initializer?.Init(this);
            Events?.PreRun(this, _config.Delta);
            var value = _calculator.Calculate(_config.Delta, x, y, z);
            _validator.Validate(value);
            Events?.PostRun(this, value);
            return value;
        }

    }
}
