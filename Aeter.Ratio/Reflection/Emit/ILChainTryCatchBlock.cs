/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILChainTryCatchBlock
    {
        private readonly ILGenerator _il;
        private readonly Label _label;

        public ILChainTryCatchBlock(ILGenerator il, Action<ILGenerator> tryBlockGenerator)
        {
            _il = il;
            _label = _il.BeginExceptionBlock();
            tryBlockGenerator.Invoke(il);
        }

        public ILChainTryCatchBlock(ILGenerator il, Action tryBlockGenerator)
        {
            _il = il;
            _label = _il.BeginExceptionBlock();
            tryBlockGenerator.Invoke();
        }

        public ILChainTryCatchBlock Catch(Type exceptionType, Action<ILGenerator> catchBlockGenerator)
        {
            _il.BeginCatchBlock(exceptionType);
            catchBlockGenerator.Invoke(_il);
            return this;
        }

        public ILChainTryCatchBlock Catch(Type exceptionType, Action catchBlockGenerator)
        {
            _il.BeginCatchBlock(exceptionType);
            catchBlockGenerator.Invoke();
            return this;
        }

        public ILChainTryCatchBlock Finally(Action<ILGenerator> finallyBlockGenerator)
        {
            _il.BeginFinallyBlock();
            finallyBlockGenerator.Invoke(_il);
            return this;
        }

        public ILChainTryCatchBlock Finally(Action finallyBlockGenerator)
        {
            _il.BeginFinallyBlock();
            finallyBlockGenerator.Invoke();
            return this;
        }

        public void End()
        {
            _il.EndExceptionBlock();
        }


    }
}