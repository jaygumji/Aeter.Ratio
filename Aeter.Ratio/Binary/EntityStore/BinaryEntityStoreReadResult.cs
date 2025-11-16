/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Represents the result of <see cref="BinaryEntityStore.ReadAsync"/> which couples metadata with a payload buffer.
    /// </summary>
    public sealed class BinaryEntityStoreReadResult : IDisposable
    {
        public BinaryEntityStoreReadResult(BinaryReadBuffer buffer, BinaryEntityStoreEntryHeader header)
        {
            Buffer = buffer;
            Header = header;
        }

        /// <summary>
        /// Gets the buffer positioned at the payload.
        /// </summary>
        public BinaryReadBuffer Buffer { get; }
        /// <summary>
        /// Gets the parsed entry header.
        /// </summary>
        public BinaryEntityStoreEntryHeader Header { get; }

        /// <summary>
        /// Returns the buffer to its pool.
        /// </summary>
        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
