/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Describes a reusable block of IL instructions.
    /// </summary>
    public interface IILSnippet
    {
        /// <summary>
        /// Emits the snippet into the supplied generator.
        /// </summary>
        void Generate(ILGenerator il);
    }

    /// <summary>
    /// Base class that simplifies ILSnippet implementations.
    /// </summary>
    public abstract class ILSnippet : IILSnippet
    {
        void IILSnippet.Generate(ILGenerator il)
        {
            OnGenerate(il);
        }

        /// <summary>
        /// When implemented, emits the snippet body.
        /// </summary>
        protected abstract void OnGenerate(ILGenerator il);

        /// <summary>
        /// Builds a snippet that calls a static method.
        /// </summary>
        public static ILCallMethodSnippet Call(MethodInfo method, params ILPointer[] parameters)
        {
            return new ILCallMethodSnippet(method, parameters);
        }

        /// <summary>
        /// Builds a snippet that calls an instance method on <paramref name="instance"/>.
        /// </summary>
        public static ILCallMethodSnippet Call(ILPointer instance, MethodInfo method, params ILPointer[] parameters)
        {
            return new ILCallMethodSnippet(instance, method, parameters);
        }

        /// <summary>
        /// Builds a snippet that calls a named method on the provided <paramref name="instance"/>.
        /// </summary>
        public static ILCallMethodSnippet Call(ILPointer instance, string methodName, params ILPointer[] parameters)
        {
            if (instance == null) {
                throw new ArgumentNullException(nameof(instance));
            }
            if (instance.Type == null) {
                throw new ArgumentException("The instance type is missing.");
            }
            var method = instance.Type.GetMethod(methodName);
            if (method == null) {
                throw new ArgumentException($"The method {methodName} could not be found on type {instance.Type.FullName}");
            }
            return new ILCallMethodSnippet(instance, method, parameters);
        }
    }
}
