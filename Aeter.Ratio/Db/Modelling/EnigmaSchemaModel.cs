/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Db.Modelling
{
    /// <summary>
    /// Enigma schema model
    /// </summary>
    public class EnigmaSchemaModel
    {

        private readonly Dictionary<Type, EnigmaSchemaEntity> _entities; 

        public EnigmaSchemaModel()
        {
            _entities = new Dictionary<Type, EnigmaSchemaEntity>();
        }

        /// <summary>Gets the entities.</summary>
        /// <value>The entities.</value>
        public IEnumerable<EnigmaSchemaEntity> Entities => _entities.Values;

        public EnigmaSchemaEntity SchemaFor(Type entityType)
        {
            return _entities[entityType];
        }
    }
}