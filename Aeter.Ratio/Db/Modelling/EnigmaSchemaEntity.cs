/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;

namespace Aeter.Ratio.Db.Modelling
{

    public class EnigmaSchemaEntity
    {
        public EnigmaSchemaEntity()
        {
            Properties = new List<EnigmaSchemaEntityProperty>();
            Indexes = new List<EnigmaSchemaEntityIndex>();
            Name = string.Empty;
        }

        public string Name { get; set; }

        public List<EnigmaSchemaEntityProperty> Properties { get; }

        public List<EnigmaSchemaEntityIndex> Indexes { get; } 
        
    }
}
