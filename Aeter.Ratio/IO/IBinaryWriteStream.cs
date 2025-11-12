/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Extends <see cref="IBinaryReadStream"/> with random-access write operations.
    /// </summary>
    public interface IBinaryWriteStream : IBinaryReadStream
    {
        /// <summary>
        /// Writes the specified data to the stream starting at the supplied offset.
        /// </summary>
        /// <param name="offset">Absolute byte offset at which to start writing.</param>
        /// <param name="buffer">Source data to write.</param>
        void Write(long offset, ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Asynchronously writes the specified data to the stream starting at the supplied offset.
        /// </summary>
        /// <param name="offset">Absolute byte offset at which to start writing.</param>
        /// <param name="buffer">Source data to write.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>A task that completes when all bytes have been written.</returns>
        ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
