/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Data.Tracking
{
    /// <summary>
    /// Represents the state of an entity
    /// </summary>
    public enum EntityState
    {
        /// <summary>The entity is intact</summary>
        Intact,

        /// <summary>The entity is new</summary>
        New,

        /// <summary>The entity has been modified</summary>
        Modified,

        /// <summary>The entity has been removed</summary>
        Removed
    }
}