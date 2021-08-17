/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Data.Tracking
{
    /// <summary>
    /// Entity entry representing the current state of the entity
    /// </summary>
    public interface IEntityEntry
    {
        /// <summary>Gets the key.</summary>
        /// <value>The key.</value>
        object Key { get; }

        /// <summary>Gets the entity.</summary>
        /// <value>The entity.</value>
        object Entity { get; }

        /// <summary>Gets the type of the entity.</summary>
        /// <value>The type of the entity.</value>
        Type EntityType { get; }

        /// <summary>Gets the state.</summary>
        /// <value>The state.</value>
        EntityState State { get; }
    }
}