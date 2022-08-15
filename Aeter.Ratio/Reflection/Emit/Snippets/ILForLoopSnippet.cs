/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{

    public class ILForLoopSnippet : ILSnippet
    {
        private readonly ILPointer _initialValue;
        private readonly ILPointer _increment;

        private readonly DelegatedILHandler<ILPointer> _conditionHandler;
        private readonly DelegatedILHandler<ILPointer> _bodyHandler;

        public ILForLoopSnippet(ILPointer initialValue, ILGenerationMethodHandler<ILPointer> conditionHandler,
            ILGenerationMethodHandler<ILPointer> bodyHandler, ILPointer increment)
        {
            if (initialValue.Type != increment.Type)
                throw new ArgumentException("The type of the initial value and the increment value must match");

            _initialValue = initialValue;
            _increment = increment;
            _conditionHandler = new DelegatedILHandler<ILPointer>(conditionHandler);
            _bodyHandler = new DelegatedILHandler<ILPointer>(bodyHandler);
        }

        public ILForLoopSnippet(ILPointer initialValue, ILGenerationHandler<ILPointer> conditionHandler,
            ILGenerationHandler<ILPointer> bodyHandler, ILPointer increment)
        {
            if (initialValue.Type == null) throw new ArgumentException($"{nameof(initialValue)}.{nameof(initialValue.Type)} can not be null");
            if (initialValue.Type != increment.Type)
                throw new ArgumentException("The type of the initial value and the increment value must match");

            _initialValue = initialValue;
            _increment = increment;
            _conditionHandler = new DelegatedILHandler<ILPointer>(conditionHandler);
            _bodyHandler = new DelegatedILHandler<ILPointer>(bodyHandler);
        }

        protected override void OnGenerate(ILGenerator il)
        {
            var value = il.NewLocal(_initialValue.Type!);
            il.Set(value, _initialValue);

            var labelCondition = il.NewLabel();
            labelCondition.TransferLong();

            var labelBody = il.NewLabel().Mark();

            _bodyHandler.Invoke(il, value);
            il.Increment(value, _increment);

            labelCondition.Mark();
            _conditionHandler.Invoke(il, value);

            labelBody.TransferLongIfTrue();
        }
    }

}