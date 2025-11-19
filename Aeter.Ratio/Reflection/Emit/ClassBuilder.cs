/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Wraps <see cref="TypeBuilder"/> with convenience helpers for emitting members.
    /// </summary>
    public class ClassBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private Type? _type;
        private bool _isSealed;

        /// <summary>
        /// Creates a builder that targets the specified <paramref name="typeBuilder"/>.
        /// </summary>
        /// <param name="typeBuilder">Underlying builder used for emission.</param>
        public ClassBuilder(TypeBuilder typeBuilder)
        {
            _typeBuilder = typeBuilder;
        }

        /// <summary>
        /// Gets the runtime type once <see cref="Seal"/> has been invoked; otherwise the underlying <see cref="TypeBuilder"/>.
        /// </summary>
        public Type Type { get { return _isSealed ? _type! : _typeBuilder; } }

        /// <summary>
        /// Gets a value indicating whether <see cref="Seal"/> has completed.
        /// </summary>
        public bool IsSealed { get { return _isSealed; } }

        /// <summary>
        /// Creates a private field with the supplied name and type.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <param name="fieldType">Type of the field.</param>
        public FieldInfo DefinePrivateField(string fieldName, Type fieldType)
        {
            return _typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Private);
        }

        /// <summary>
        /// Emits a public, read-only property backed by a compiler generated getter that returns <paramref name="value"/>.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Property CLR type.</param>
        /// <param name="value">Constant value returned by the getter.</param>
        public PropertyInfo DefinePublicReadOnlyProperty(string propertyName, Type propertyType, object value)
        {
            var propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var pValue = ILPointer.Guess(value);

            var getMethod = DefineMethod("get_" + propertyName, propertyType, Array.Empty<Type>());
            var il = getMethod.GetILGenerator();
            il.Load(pValue);
            il.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getMethod);
            return propertyBuilder;
        }

        /// <summary>
        /// Creates a public parameterless constructor.
        /// </summary>
        public ConstructorBuilder DefineDefaultConstructor()
        {
            return _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
        }

        /// <summary>
        /// Creates a public constructor with the specified signature.
        /// </summary>
        /// <param name="parameterTypes">Constructor parameter types.</param>
        public ConstructorBuilder DefineConstructor(params Type[] parameterTypes)
        {
            return _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);
        }

        /// <summary>
        /// Defines a public method.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameterTypes">Parameter types.</param>
        public MethodBuilder DefineMethod(string name, Type returnType, Type[] parameterTypes)
        {
            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig;
            var methodBuilder = _typeBuilder.DefineMethod(name, attributes, returnType, parameterTypes);
            return methodBuilder;
        }

        /// <summary>
        /// Defines a virtual public method intended to override or implement an interface slot.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameterTypes">Parameter types.</param>
        public MethodBuilder DefineOverloadMethod(string name, Type returnType, Type[] parameterTypes)
        {
            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            var methodBuilder = _typeBuilder.DefineMethod(name, attributes, returnType, parameterTypes);
            return methodBuilder;
        }

        /// <summary>
        /// Finalizes the type definition and prevents further modifications.
        /// </summary>
        public void Seal()
        {
            if (_isSealed) return;
            _isSealed = true;

            _type = _typeBuilder.CreateTypeInfo()!.AsType();
        }

    }
}
