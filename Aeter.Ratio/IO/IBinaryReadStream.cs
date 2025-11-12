/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Defines read capabilities for a random-access binary stream.
    /// </summary>
    public interface IBinaryReadStream : IBinaryStream
    {
        /// <summary>
        /// Reads bytes starting at the specified offset into the provided buffer.
        /// </summary>
        /// <param name="offset">Absolute byte offset from which to begin reading.</param>
        /// <param name="buffer">Destination buffer that receives the data.</param>
        /// <returns>The number of bytes read into <paramref name="buffer"/>.</returns>
        int Read(long offset, Span<byte> buffer);

        /// <summary>
        /// Asynchronously reads bytes starting at the specified offset into the provided buffer.
        /// </summary>
        /// <param name="offset">Absolute byte offset from which to begin reading.</param>
        /// <param name="buffer">Destination buffer that receives the data.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>The number of bytes read into <paramref name="buffer"/>.</returns>
        ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
