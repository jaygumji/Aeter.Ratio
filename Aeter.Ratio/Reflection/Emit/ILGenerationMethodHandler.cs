/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Represents a snippet of IL that does not require access to <see cref="ILGenerator"/>.
    /// </summary>
    public delegate void ILGenerationHandler();

    /// <summary>
    /// Represents a snippet that needs the <see cref="ILGenerator"/>.
    /// </summary>
    public delegate void ILGenerationMethodHandler(ILGenerator il);

    /// <summary>
    /// Represents a snippet that operates on the supplied value without directly touching the generator.
    /// </summary>
    public delegate void ILGenerationHandler<in T>(T value) where T : ILPointer;

    /// <summary>
    /// Represents a snippet that receives both the generator and an argument pointer.
    /// </summary>
    public delegate void ILGenerationMethodHandler<in T>(ILGenerator il, T value) where T : ILPointer;
}
