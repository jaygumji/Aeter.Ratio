/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aeter.Ratio.Reflection
{
    public class ExtendedType
    {
        private readonly Lazy<IContainerTypeInfo> _containerTypeInfo;
        private readonly Lazy<TypeClassification> _class;

        public ExtendedType(Type type) : this(type, FactoryTypeProvider.Instance)
        {

        }

        public ExtendedType(Type type, ITypeProvider provider)
        {
            Ref = type;
            Info = type.GetTypeInfo();
            _containerTypeInfo = new Lazy<IContainerTypeInfo>(type.GetContainerTypeInfo);
            _class = new Lazy<TypeClassification>(() => type.GetClassification(_containerTypeInfo.Value));
            Provider = provider;
        }

        public Type Ref { get; }
        public TypeInfo Info { get; }
        public TypeClassification Classification => _class.Value;
        public IContainerTypeInfo Container => _containerTypeInfo.Value;
        public bool ImplementsCollection => Classification == TypeClassification.Collection || Classification == TypeClassification.Dictionary;

        public ITypeProvider Provider { get; }

        public bool IsValueOrNullableOfValue()
        {
            if (Classification == TypeClassification.Value) return true;
            if (Classification != TypeClassification.Nullable) return false;
            var elementExt = Provider.Extend(Container.AsNullable().ElementType);
            return elementExt.Classification == TypeClassification.Value;
        }

        public bool IsEnum()
        {
            return Info.IsEnum || (Classification == TypeClassification.Nullable && Container.AsNullable().ElementType.GetTypeInfo().IsEnum);
        }

        public Type GetUnderlyingEnumType()
        {
            if (Info.IsEnum) return Enum.GetUnderlyingType(Ref);

            if (Classification == TypeClassification.Nullable) {
                var elementType = Container.AsNullable().ElementType;
                if (elementType.GetTypeInfo().IsEnum) {
                    var underlyingType = Enum.GetUnderlyingType(elementType);
                    return typeof (Nullable<>).MakeGenericType(underlyingType);
                }
            }

            throw new InvalidOperationException("The type is not an enum");
        }

        public bool TryGetArrayTypeInfo(out ArrayContainerTypeInfo arrayTypeInfo)
        {
            arrayTypeInfo = _containerTypeInfo.Value as ArrayContainerTypeInfo;
            return arrayTypeInfo != null;
        }

        public bool TryGetCollectionTypeInfo(out CollectionContainerTypeInfo collectionTypeInfo)
        {
            collectionTypeInfo = _containerTypeInfo.Value as CollectionContainerTypeInfo;
            return collectionTypeInfo != null;
        }

        public bool TryGetDictionaryTypeInfo(out DictionaryContainerTypeInfo dictionaryTypeInfo)
        {
            dictionaryTypeInfo = _containerTypeInfo.Value as DictionaryContainerTypeInfo;
            return dictionaryTypeInfo != null;
        }

    }

}
