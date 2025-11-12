/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Represents a binary stream that exposes position and length metadata as well as flush semantics.
    /// </summary>
    public interface IBinaryStream : IDisposable
    {
        /// <summary>
        /// Gets the current position in the underlying stream.
        /// </summary>
        long Position { get; }

        /// <summary>
        /// Gets the total length of the underlying stream.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Flushes any buffered data to the underlying storage.
        /// </summary>
        void Flush();
    }
}
