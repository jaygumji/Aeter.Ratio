/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Defines optional behaviors a binary entity store index may expose.
    /// </summary>
    [Flags]
    public enum BinaryEntityStoreIndexCapabilities
    {
        /// <summary>
        /// No additional capabilities.
        /// </summary>
        None = 0,
        /// <summary>
        /// Index entries are stored in an ordered fashion that enables binary/equality searches.
        /// </summary>
        BinarySearch = 1,
        /// <summary>
        /// Index stores the string catalog necessary for fuzzy full-text searches.
        /// </summary>
        FullText = 2
    }
}
