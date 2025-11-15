/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// Arguments passed to <see cref="BinaryEntityStore.ReadAllAsync(Func{BinaryEntityStoreReadAllArgs, Task}, object?, CancellationToken)"/>.
    /// </summary>
    public class BinaryEntityStoreReadAllArgs(BinaryEntityStore store, long offset, BinaryEntityStoreEntryHeader header, object? state)
    {
        /// <summary>
        /// Gets the store that produced the entry.
        /// </summary>
        public BinaryEntityStore Store { get; } = store;
        /// <summary>
        /// Gets the absolute offset of the entry.
        /// </summary>
        public long Offset { get; } = offset;
        /// <summary>
        /// Gets the parsed header.
        /// </summary>
        public BinaryEntityStoreEntryHeader Header { get; } = header;
        /// <summary>
        /// Gets the optional state object passed to <see cref="BinaryEntityStore.ReadAllAsync"/>.
        /// </summary>
        public object? State { get; } = state;
    }
}
