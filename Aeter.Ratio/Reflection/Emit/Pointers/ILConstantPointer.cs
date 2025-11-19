/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Base class for pointers representing constant literals.
    /// </summary>
    public abstract class ILConstantPointer : ILPointer
    {
        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// Creates the pointer for the supplied constant type.
        /// </summary>
        protected ILConstantPointer(Type type)
        {
            Type = type;
        }

        /// <inheritdoc />
        protected override void LoadAddress(ILGenerator il)
        {
            Load(il);
        }
    }
}
