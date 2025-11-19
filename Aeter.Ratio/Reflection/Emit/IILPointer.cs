/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Represents a value that can load itself onto an <see cref="ILGenerator"/> evaluation stack.
    /// </summary>
    public interface IILPointer
    {
        /// <summary>
        /// Gets the managed type of the pointer when known.
        /// </summary>
        Type? Type { get; }

        /// <summary>
        /// Emits IL instructions that push the value represented by this pointer onto the stack.
        /// </summary>
        void Load(ILGenerator il);
        /// <summary>
        /// Emits IL instructions that push the address of the pointer target onto the stack.
        /// </summary>
        void LoadAddress(ILGenerator il);
    }
}
