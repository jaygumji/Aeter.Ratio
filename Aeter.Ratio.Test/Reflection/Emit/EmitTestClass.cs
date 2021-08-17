/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection;

namespace Aeter.Ratio.Test.Reflection.Emit
{
    public class EmitTestClass : IEmitTest
    {
        public static readonly ConstructorInfo ConstructorInfo
            = typeof(EmitTestClass).GetConstructor(new[] { typeof(string) });

        public string Message { get; }

        public EmitTestClass(string msg)
        {
            Message = msg;
        }
    }
}