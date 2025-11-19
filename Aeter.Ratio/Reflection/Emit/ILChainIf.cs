/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Fluent helper for emitting conditional branches.
    /// </summary>
    public class ILChainIf
    {
        private readonly ILGenerator _il;

        /// <summary>
        /// Initializes the chain for the specified generator.
        /// </summary>
        public ILChainIf(ILGenerator il)
        {
            _il = il;
        }

        /// <summary>
        /// Gets or sets the delegate that emits the condition logic (should leave a bool on the stack).
        /// </summary>
        public ILGenerationHandler? Condition { get; set; }
        /// <summary>
        /// Gets or sets the delegate that emits the "then" branch.
        /// </summary>
        public ILGenerationHandler? Body { get; set; }
        /// <summary>
        /// Gets or sets the optional "else" branch delegate.
        /// </summary>
        public ILGenerationHandler? ElseBody { get; set; }

        /// <summary>
        /// Emits the if/else structure using the configured handlers.
        /// </summary>
        public void End()
        {
            if (Condition == null) throw new InvalidOperationException("No condition set");
            if (Body == null) throw new InvalidOperationException("No body set");

            Condition.Invoke();

            var endLabel = _il.NewLabel();

            if (ElseBody == null) {
                endLabel.TransferLongIfFalse();
                Body.Invoke();
            }
            else {
                var elseLabel = _il.NewLabel();
                elseLabel.TransferLongIfFalse();
                Body.Invoke();
                endLabel.TransferLong();

                elseLabel.Mark();
                ElseBody.Invoke();
            }
            endLabel.Mark();
        }
    }
}
