/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            if (!Container.AsNullable(out var nullable)) return false;
            var elementExt = Provider.Extend(nullable.ElementType);
            return elementExt.Classification == TypeClassification.Value;
        }

        public bool IsEnum()
        {
            return Info.IsEnum || (Container.AsNullable(out var nullable) && nullable.ElementType.IsEnum);
        }

        public Type GetUnderlyingEnumType()
        {
            if (Info.IsEnum) return Enum.GetUnderlyingType(Ref);

            if (Container.AsNullable(out var nullable)) {
                var elementType = nullable.ElementType;
                if (elementType.IsEnum) {
                    var underlyingType = Enum.GetUnderlyingType(elementType);
                    return typeof (Nullable<>).MakeGenericType(underlyingType);
                }
            }

            throw new InvalidOperationException("The type is not an enum");
        }

        public bool TryGetNullableTypeInfo([MaybeNullWhen(false)] out NullableContainerTypeInfo nullableTypeInfo)
        {
            return _containerTypeInfo.Value.AsNullable(out nullableTypeInfo);
        }

        public bool TryGetArrayTypeInfo([MaybeNullWhen(false)] out ArrayContainerTypeInfo arrayTypeInfo)
        {
            return _containerTypeInfo.Value.AsArray(out arrayTypeInfo);
        }

        public bool TryGetCollectionTypeInfo([MaybeNullWhen(false)] out CollectionContainerTypeInfo collectionTypeInfo)
        {
            return _containerTypeInfo.Value.AsCollection(out collectionTypeInfo);
        }

        public bool TryGetDictionaryTypeInfo([MaybeNullWhen(false)] out DictionaryContainerTypeInfo dictionaryTypeInfo)
        {
            return _containerTypeInfo.Value.AsDictionary(out dictionaryTypeInfo);
        }

    }

}
