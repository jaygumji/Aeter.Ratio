/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{

    public class ILEnumerateSnippet : ILSnippet
    {
        private readonly ILPointer _enumerable;
        private readonly DelegatedILHandler<ILPointer> _iterateBody;

        private static readonly MethodInfo EnumeratorMoveNext;
        private static readonly MethodInfo DisposableDispose;

        static ILEnumerateSnippet()
        {
            EnumeratorMoveNext = typeof(IEnumerator).FindMethod(nameof(IEnumerator.MoveNext));
            DisposableDispose = typeof(IDisposable).FindMethod(nameof(IDisposable.Dispose));
        }

        public ILEnumerateSnippet(ILPointer enumerable, ILGenerationMethodHandler<ILPointer> iterateBody)
        {
            _enumerable = enumerable;
            _iterateBody = new DelegatedILHandler<ILPointer>(iterateBody);
        }

        public ILEnumerateSnippet(ILPointer enumerable, ILGenerationHandler<ILPointer> iterateBody)
        {
            _enumerable = enumerable;
            _iterateBody = new DelegatedILHandler<ILPointer>(iterateBody);
        }

        protected override void OnGenerate(ILGenerator il)
        {
            if (!_enumerable.Type!.TryGetInterface(typeof(IEnumerable<>), out var enumerableType)) {
                throw new InvalidOperationException("Could not enumerate the type " + _enumerable.Type!.FullName);
            }

            var getEnumerator = ILSnippet.Call(_enumerable, enumerableType.GetRuntimeMethod("GetEnumerator", Type.EmptyTypes)!);

            var elementType = enumerableType.GetGenericArguments()[0];
            var itLocal = il.NewLocal(getEnumerator.ReturnType);
            il.Set(itLocal, getEnumerator);

            il.Try(() => {
                var itHeadLabel = il.NewLabel();
                var itBodyLabel = il.NewLabel();
                itHeadLabel.TransferLong();
                var itVarLocal = il.NewLocal(elementType);

                itBodyLabel.Mark();
                il.Set(itVarLocal, ILPointer.Property(itLocal, "Current"));

                _iterateBody.Invoke(il, itVarLocal);

                itHeadLabel.Mark();
                il.InvokeMethod(itLocal, EnumeratorMoveNext);

                itBodyLabel.TransferLongIfTrue();
            })
            .Finally(() => {
                var endLabel = il.NewLabel();
                endLabel.TransferIfNull(itLocal);

                il.InvokeMethod(itLocal, DisposableDispose);

                endLabel.Mark();
            })
            .End();
        }

    }
}
