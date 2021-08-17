/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Data.Tracking
{
    /// <summary>
    /// Enables editing of an entity entry
    /// </summary>
    public interface IEditableEntityEntry
    {
        /// <summary>Modifies the state.</summary>
        /// <param name="newState">The new state.</param>
        void ModifyState(EntityState newState);
    }
}