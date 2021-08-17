/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILLabel
    {
        private readonly ILGenerator _il;
        private readonly Label _label;

        public ILLabel(ILGenerator il) : this(il, il.DefineLabel())
        {
            
        }

        public ILLabel(ILGenerator il, Label label)
        {
            _il = il;
            _label = label;
        }

        public ILLabel Mark()
        {
            _il.MarkLabel(_label);
            return this;
        }

        public void TransferLong()
        {
            _il.Emit(OpCodes.Br, _label);
        }

        public void TransferLongIfFalse()
        {
            _il.Emit(OpCodes.Brfalse, _label);
        }

        public void TransferLongIfTrue()
        {
            _il.Emit(OpCodes.Brtrue, _label);
        }

        public void TransferLongIfTrue(ILPointer value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Type != typeof(bool)) {
                throw new ArgumentException("The type of the pointer must be boolean.");
            }

            _il.Load(value);
            TransferLongIfTrue();
        }

        public void TransferShort()
        {
            _il.Emit(OpCodes.Br_S, _label);
        }

        public void TransferShortIfFalse()
        {
            _il.Emit(OpCodes.Brfalse_S, _label);
        }

        public void TransferShortIfTrue()
        {
            _il.Emit(OpCodes.Brtrue_S, _label);
        }

        public void TransferIfNull(ILPointer value)
        {
            _il.AreEqual(value, null);
            TransferLongIfTrue();
        }

        public void TransferIfNotNull(ILPointer value)
        {
            _il.AreEqual(value, null);
            TransferLongIfFalse();
        }

    }
}

