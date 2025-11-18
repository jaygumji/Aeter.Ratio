/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Identifies the type of mutation persisted inside an index log.
    /// </summary>
    internal enum BinaryEntityStoreIndexMutationType : byte
    {
        /// <summary>
        /// Adds a value for an entity.
        /// </summary>
        Add = 1,
        /// <summary>
        /// Removes every value owned by an entity.
        /// </summary>
        Remove = 2
    }
}
