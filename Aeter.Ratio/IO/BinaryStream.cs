/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.IO
{
    public class BinaryStream
    {
        public static IBinaryWriteStream Passthrough(Stream stream) => new PassthroughStreamImpl(stream);
        public static IBinaryWriteStream MemoryStream(MemoryStream? stream = default) => new MemoryStreamImpl(stream);
        public static IBinaryWriteStream ParallellFileStream(string path) => new ParallellFileStreamImpl(path);

        private class PassthroughStreamImpl : IBinaryWriteStream
        {
            public PassthroughStreamImpl(Stream stream)
            {
                Stream = stream;
            }

            private Stream Stream { get; }
            public long Position => Stream.Position;
            public long Length => Stream.Length;

            public int Read(long offset, Span<byte> buffer)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.Read(buffer);
            }

            public ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.ReadAsync(buffer, cancellationToken);
            }

            public void Dispose()
            {
                Stream.Dispose();
            }

            public void Write(long offset, ReadOnlySpan<byte> buffer)
            {
                Stream.Seek((int)offset, SeekOrigin.Begin);
                Stream.Write(buffer);
            }

            public ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.WriteAsync(buffer, cancellationToken);
            }

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
            public long Position => Stream.Position;
            public long Length => Stream.Length;

            public int Read(long offset, Span<byte> buffer)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.Read(buffer);
            }

            public ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.ReadAsync(buffer, cancellationToken);
            }

            public void Dispose()
            {
                Stream.Dispose();
            }

            public void Write(long offset, ReadOnlySpan<byte> buffer)
            {
                Stream.Seek((int)offset, SeekOrigin.Begin);
                Stream.Write(buffer);
            }

            public ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Stream.Seek(offset, SeekOrigin.Begin);
                return Stream.WriteAsync(buffer, cancellationToken);
            }

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
                    FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite,
                    bufferSize: 1, // buffer not used by RandomAccess
                    FileOptions.Asynchronous | FileOptions.RandomAccess);
            }

            private FileStream Stream { get; }
            public long Position => Stream.Position;
            public long Length => Stream.Length;

            public int Read(long offset, Span<byte> buffer)
            {
                return RandomAccess.Read(Stream.SafeFileHandle, buffer, offset);
            }

            public ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return RandomAccess.ReadAsync(Stream.SafeFileHandle, buffer, offset, cancellationToken);
            }

            public void Dispose()
            {
                Stream.Dispose();
            }

            public void Write(long offset, ReadOnlySpan<byte> buffer)
            {
                RandomAccess.Write(Stream.SafeFileHandle, buffer, offset);
            }

            public ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return RandomAccess.WriteAsync(Stream.SafeFileHandle, buffer, offset, cancellationToken);
            }

            public void Flush()
            {
                RandomAccess.FlushToDisk(Stream.SafeFileHandle);
            }

        }
    }
}
