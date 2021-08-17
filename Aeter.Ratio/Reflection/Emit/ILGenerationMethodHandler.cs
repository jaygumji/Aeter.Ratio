/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public delegate void ILGenerationHandler();
    public delegate void ILGenerationMethodHandler(ILGenerator il);

    public delegate void ILGenerationHandler<in T>(T value) where T : ILPointer;
    public delegate void ILGenerationMethodHandler<in T>(ILGenerator il, T value) where T : ILPointer;
}