/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Represents a pointer that can also be assigned to in IL.
    /// </summary>
    public interface IILVariable : IILPointer
    {
        /// <summary>
        /// Emits any instructions required before storing into the variable (for example loading the address).
        /// </summary>
        void PreSet(ILGenerator il);
        /// <summary>
        /// Emits instructions that store the top-of-stack value in the variable.
        /// </summary>
        void Set(ILGenerator il);
    }
}
