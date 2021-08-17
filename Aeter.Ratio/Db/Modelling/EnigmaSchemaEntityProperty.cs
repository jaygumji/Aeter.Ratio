/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Db.Modelling
{
    public class EnigmaSchemaEntityProperty
    {
        public string Name { get; set; }
        public int FieldIndex { get; set; }
        public TypeClassification TypeClassification { get; set; }
        public StrictValueType? Type { get; set; }
        public string CustomType { get; set; }
    }
}