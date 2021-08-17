/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Xunit;

namespace Aeter.Ratio.Test
{
    public class DynamicActivatorTests
    {

        public class ActivationClass
        {
            public int Constructor { get; }
            public ActivationClass() { Constructor = 0; }
            public ActivationClass(int x, int y, string message) { Constructor = 1; }
        }

        [Fact]
        public void ActivateParameterLess()
        {
            var activator = new DynamicActivator(typeof(ActivationClass));
            var instance = activator.Activate() as ActivationClass;
            Assert.NotNull(instance);
            Assert.Equal(0, instance.Constructor);
        }

        [Fact]
        public void ActivateStandardTypeParameters()
        {
            var activator = new DynamicActivator(typeof(ActivationClass), typeof(int), typeof(int), typeof(string));
            var instance = activator.Activate(1, 2, "Hello World") as ActivationClass;
            Assert.NotNull(instance);
            Assert.Equal(1, instance.Constructor);
        }

        public object Activate(object[] parameters)
        {
            return new ActivationClass((int)parameters[0], (int)parameters[1], (string)parameters[2]);
        }

        public void Test()
        {
            var x = 1L;
            var a1 = (byte)x;
            var a2 = (short)x;
            var a3 = (int)x;
            var a4 = (float)x;
            var a5 = (double)x;
            var a6 = (ushort)x;
            var a7 = (uint)x;
            var a8 = (ulong)x;
            var a9 = (decimal)x;

            Assert.Equal(x, a1);
        }
    }
}
