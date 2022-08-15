/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Db.Modelling
{
    public class EnigmaSchemaEntityProperty
    {
        public EnigmaSchemaEntityProperty(string name, int fieldIndex, TypeClassification typeClassification, StrictValueType? type, string customType)
        {
            Name = name;
            FieldIndex = fieldIndex;
            TypeClassification = typeClassification;
            Type = type;
            CustomType = customType;
        }

        public string Name { get; }
        public int FieldIndex { get; }
        public TypeClassification TypeClassification { get; }
        public StrictValueType? Type { get; }
        public string CustomType { get; }
    }
}