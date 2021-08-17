/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILWhileLoopSnippet : ILSnippet
    {
        private readonly DelegatedILHandler _conditionHandler;
        private readonly DelegatedILHandler _bodyHandler;

        public ILWhileLoopSnippet(ILGenerationMethodHandler conditionHandler, ILGenerationMethodHandler bodyHandler)
        {
            _conditionHandler = new DelegatedILHandler(conditionHandler);
            _bodyHandler = new DelegatedILHandler(bodyHandler);
        }

        public ILWhileLoopSnippet(ILGenerationHandler conditionHandler, ILGenerationHandler bodyHandler)
        {
            _conditionHandler = new DelegatedILHandler(conditionHandler);
            _bodyHandler = new DelegatedILHandler(bodyHandler);
        }

        protected override void OnGenerate(ILGenerator il)
        {
            var loopConditionLabel = il.NewLabel();
            var loopBodyLabel = il.NewLabel();

            loopConditionLabel.TransferLong();
            loopBodyLabel.Mark();
            _bodyHandler.Invoke(il);

            loopConditionLabel.Mark();
            _conditionHandler.Invoke(il);

            loopBodyLabel.TransferLongIfTrue();
        }
    }
}