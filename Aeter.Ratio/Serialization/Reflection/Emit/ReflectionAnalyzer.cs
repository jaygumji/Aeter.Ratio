/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public static class ReflectionAnalyzer
    {

        public static bool TryGetComplexTypes(ExtendedType type, out Type[] types)
        {
            if (type.Classification == TypeClassification.Complex) {
                types = new[] { type.Ref };
                return true;
            }
            if (type.Classification == TypeClassification.Nullable) {
                var elementType = type.Provider.Extend(type.Container.AsNullable().ElementType);
                return TryGetComplexTypes(elementType, out types);
            }
            if (type.Classification == TypeClassification.Dictionary) {
                var container = type.Container.AsDictionary();
                var keyTypeExt = type.Provider.Extend(container.KeyType);
                var hasKeyTypes = TryGetComplexTypes(keyTypeExt, out var keyTypes);

                var valueTypeExt = type.Provider.Extend(container.ValueType);
                var hasValueTypes = TryGetComplexTypes(valueTypeExt, out var valueTypes);

                if (!hasKeyTypes && !hasValueTypes) {
                    types = null;
                    return false;
                }

                if (hasKeyTypes && hasValueTypes) types = keyTypes.Concat(valueTypes).ToArray();
                else if (hasKeyTypes) types = keyTypes;
                else types = valueTypes;

                return true;
            }
            if (type.Classification == TypeClassification.Collection) {
                var elementTypeExt = type.Provider.Extend(type.Container.AsCollection().ElementType);
                return TryGetComplexTypes(elementTypeExt, out types);
            }

            types = null;
            return false;
        }

    }
}
