/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILChainIf
    {
        private readonly ILGenerator _il;

        public ILChainIf(ILGenerator il)
        {
            _il = il;
        }

        public ILGenerationHandler Condition { get; set; }
        public ILGenerationHandler Body { get; set; }
        public ILGenerationHandler ElseBody { get; set; }

        public void End()
        {
            if (Condition == null) throw new InvalidOperationException("No condition set");
            if (Body == null) throw new InvalidOperationException("No body set");

            Condition.Invoke();

            var endLabel = _il.NewLabel();

            var elseLabel = default(ILLabel);
            if (ElseBody != null) {
                elseLabel = _il.NewLabel();
                elseLabel.TransferLongIfFalse();
            }
            else {
                endLabel.TransferLongIfFalse();
            }

            Body.Invoke();

            if (ElseBody != null) {
                endLabel.TransferLong();

                elseLabel.Mark();
                ElseBody.Invoke();
            }
            
            endLabel.Mark();
        }
    }
}