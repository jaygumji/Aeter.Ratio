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
    /// Factory helpers for creating <see cref="IBinaryWriteStream"/> instances over different backings.
    /// </summary>
    public class BinaryStream
    {
        /// <summary>
        /// Wraps an existing <see cref="Stream"/> and exposes it as an <see cref="IBinaryWriteStream"/>.
        /// </summary>
        /// <param name="stream">The underlying stream to wrap.</param>
        /// <returns>A binary stream that forwards all operations to <paramref name="stream"/>.</returns>
        public static IBinaryWriteStream Passthrough(Stream stream) => new PassthroughStreamImpl(stream);

        /// <summary>
        /// Creates an <see cref="IBinaryWriteStream"/> backed by a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">Optional existing memory stream. When omitted, a new instance is created.</param>
        /// <returns>A binary stream backed by the supplied or newly created memory stream.</returns>
        public static IBinaryWriteStream MemoryStream(MemoryStream? stream = default) => new MemoryStreamImpl(stream);

        /// <summary>
        /// Creates an <see cref="IBinaryWriteStream"/> that performs random-access I/O against a file path.
        /// </summary>
        /// <param name="path">Destination file path.</param>
        /// <returns>A binary stream that uses <see cref="RandomAccess"/> APIs for throughput.</returns>
        public static IBinaryWriteStream ParallellFileStream(string path) => new ParallellFileStreamImpl(path);

        /// <summary>
        /// Simple pass-through adapter for <see cref="Stream"/>.
        /// </summary>
        private class PassthroughStreamImpl : IBinaryWriteStream
        {
            /// <summary>
            /// Initializes a new instance that wraps the provided stream.
            /// </summary>
            /// <param name="stream">The underlying stream.</param>
            public PassthroughStreamImpl(Stream stream)
            {
                Stream = stream;
            }

            private Stream Stream { get; }
            /// <inheritdoc />
            public long Position => Stream.Position;
            /// <inheritdoc />
            public long Length => Stream.Length;

            /// <inheritdoc />
            public int Read(long offset, Span<byte> buffer)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.Read(buffer);
            }

            /// <inheritdoc />
            public ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.ReadAsync(buffer, cancellationToken);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Stream.Dispose();
            }

            /// <inheritdoc />
            public void Write(long offset, ReadOnlySpan<byte> buffer)
            {
                Stream.Seek((int)offset, SeekOrigin.Begin);
                Stream.Write(buffer);
            }

            /// <inheritdoc />
            public ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.WriteAsync(buffer, cancellationToken);
            }

            /// <inheritdoc />
            public void Flush()
            {
                Stream.Flush();
            }
        }
        private class MemoryStreamImpl : IBinaryWriteStream
        {
            public MemoryStreamImpl(MemoryStream? stream)
            {
                Stream = stream ?? new MemoryStream();
            }

            private MemoryStream Stream { get; }
            /// <inheritdoc />
            public long Position => Stream.Position;
            /// <inheritdoc />
            public long Length => Stream.Length;

            /// <inheritdoc />
            public int Read(long offset, Span<byte> buffer)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.Read(buffer);
            }

            /// <inheritdoc />
            public ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.ReadAsync(buffer, cancellationToken);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Stream.Dispose();
            }

            /// <inheritdoc />
            public void Write(long offset, ReadOnlySpan<byte> buffer)
            {
                Stream.Seek((int)offset, SeekOrigin.Begin);
                Stream.Write(buffer);
            }

            /// <inheritdoc />
            public ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.WriteAsync(buffer, cancellationToken);
            }

            /// <inheritdoc />
            public void Flush()
            {
                Stream.Flush();
            }
        }
        private class ParallellFileStreamImpl : IBinaryWriteStream
        {
            public ParallellFileStreamImpl(string path)
            {
                Stream = new FileStream(path,
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite,
                    bufferSize: 1, // buffer not used by RandomAccess
                    FileOptions.Asynchronous | FileOptions.RandomAccess);
            }

            private FileStream Stream { get; }
            /// <inheritdoc />
            public long Position => Stream.Position;
            /// <inheritdoc />
            public long Length => Stream.Length;

            /// <inheritdoc />
            public int Read(long offset, Span<byte> buffer)
            {
                return RandomAccess.Read(Stream.SafeFileHandle, buffer, offset);
            }

            /// <inheritdoc />
            public ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return RandomAccess.ReadAsync(Stream.SafeFileHandle, buffer, offset, cancellationToken);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Stream.Dispose();
            }

            /// <inheritdoc />
            public void Write(long offset, ReadOnlySpan<byte> buffer)
            {
                RandomAccess.Write(Stream.SafeFileHandle, buffer, offset);
            }

            /// <inheritdoc />
            public ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return RandomAccess.WriteAsync(Stream.SafeFileHandle, buffer, offset, cancellationToken);
            }

            /// <inheritdoc />
            public void Flush()
            {
                RandomAccess.FlushToDisk(Stream.SafeFileHandle);
            }

        }
    }
}
