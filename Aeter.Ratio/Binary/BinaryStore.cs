/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aeter.Ratio.IO;
using Aeter.Ratio.Threading;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// A synchronized binary store of data, guaranteed threadsafe.
    /// </summary>
    /// <seealso cref="Enigma.Db.Store.Binary.IBinaryStore" />
    /// <seealso cref="System.IDisposable" />
    public class BinaryStore : IBinaryStore, IDisposable
    {

        private readonly IWriteStream _writeStream;
        private readonly IWriteStream _offsetWriteStream;
        private readonly IStreamProvider _provider;
        private long _currentOffset;
        private long _lastFlushOffset;
        private readonly long _start;
        private readonly long _maxLength;
        private readonly SemaphoreSlim _writeSemaphore = new(1);
        private readonly Lock<long> _writeOffsetLock = new();

        public BinaryStore(IStreamProvider provider)
            : this(provider, 0, 0)
        {
        }

        private BinaryStore(IStreamProvider provider, long start, long maxLength)
        {
            _writeStream = provider.AcquireWriteStream();

            _provider = provider;
            _start = start;
            _maxLength = maxLength;

            if (_writeStream.Length > start) {
                using var readStream = _provider.AcquireReadStream();
                readStream.Seek(start, SeekOrigin.Begin);
                var offsetBuffer = new byte[8];
                readStream.Read(offsetBuffer, 0, offsetBuffer.Length);
                _currentOffset = BitConverter.ToInt64(offsetBuffer, 0);
            }
            else {
                _currentOffset = 8;
                var offsetBuffer = BitConverter.GetBytes(_currentOffset);
                _writeStream.Seek(start, SeekOrigin.Begin);
                _writeStream.Write(offsetBuffer, 0, offsetBuffer.Length);

                if (maxLength > 0) {
                    var requiredFileSize = _start + _maxLength;
                    var buffer = new byte[requiredFileSize - _writeStream.Length - 8];
                    _writeStream.Write(buffer, 0, buffer.Length);
                }
            }

            _lastFlushOffset = _currentOffset;
            _writeStream.Seek(start + _currentOffset, SeekOrigin.Begin);

            _offsetWriteStream = provider.AcquireWriteStream();
            _offsetWriteStream.Seek(start, SeekOrigin.Begin);
        }

        public bool IsEmpty { get { return _writeStream.Length <= (_start + 8); } }

        public long Size => _currentOffset - 8;

        public bool IsSpaceAvailable(long length)
        {
            if (_maxLength <= 0) return true;

            return _currentOffset + length <= _maxLength;
        }

        private async Task UpdateOffsetAsync(CancellationToken cancellationToken = default)
        {
            var offsetBuffer = BitConverter.GetBytes(_currentOffset);
            await _offsetWriteStream.WriteAsync(offsetBuffer, 0, offsetBuffer.Length, cancellationToken);
            _offsetWriteStream.Seek(-8, SeekOrigin.Current);
        }

        public async Task WriteAsync(long storeOffset, byte[] data, CancellationToken cancellationToken = default)
        {
            var handle = await _writeOffsetLock.EnterAsync(storeOffset, cancellationToken);
            try {
                using var offsetWriteStream = _provider.AcquireWriteStream();
                offsetWriteStream.Seek(storeOffset, SeekOrigin.Begin);
                await offsetWriteStream.WriteAsync(data, 0, data.Length, cancellationToken);
                if (_lastFlushOffset > storeOffset)
                    _lastFlushOffset = storeOffset - 1;
            }
            finally {
                await handle.ReleaseAsync();
            }
        }

        public async Task<(bool IsSuccessful, long Offset)> TryWriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await _writeSemaphore.WaitAsync(cancellationToken: cancellationToken);
            try {
                if (!IsSpaceAvailable(data.Length)) {
                    return (false, 0);
                }

                var storeOffset = _currentOffset;
                await _writeStream.WriteAsync(data, 0, data.Length, cancellationToken);
                _writeStream.Flush();
                _currentOffset += data.Length;
                await UpdateOffsetAsync(cancellationToken);
                return (true, storeOffset);
            }
            finally {
                _writeSemaphore.Release();
            }
        }

        private async Task EnsureFlushedAsync(long offset, CancellationToken cancellationToken = default)
        {
            if (offset < _lastFlushOffset) return;

            await _writeSemaphore.WaitAsync(cancellationToken: cancellationToken);
            try {
                if (offset < _lastFlushOffset) return;

                _writeStream.FlushForced();
                _lastFlushOffset = _currentOffset;
            }
            finally {
                _writeSemaphore.Release();
            }
        }

        public async Task<(byte[] Data, long Offset)> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            await EnsureFlushedAsync(_currentOffset, cancellationToken);

            if (_currentOffset <= _start + 8) return (Array.Empty<byte>(), 0);

            var buffer = new byte[_currentOffset - _start - 8];
            using (var readStream = _provider.AcquireReadStream()) {
                readStream.Seek(_start + 8, SeekOrigin.Begin);
                await readStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            }
            return (buffer, 0);
        }

        public async Task<byte[]> ReadAsync(long storeOffset, long length, CancellationToken cancellationToken = default)
        {
            await EnsureFlushedAsync(storeOffset, cancellationToken);

            var buffer = new byte[length];
            using (var readStream = _provider.AcquireReadStream()) {
                readStream.Seek(storeOffset, SeekOrigin.Begin);
                await readStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            }
            return buffer;
        }

        public async Task TruncateToAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await _writeSemaphore.WaitAsync(cancellationToken: cancellationToken);
            try {
                _currentOffset = 8 + data.Length;
                var offsetBuffer = BitConverter.GetBytes(_currentOffset);
                _writeStream.Seek(_start, SeekOrigin.Begin);
                await _writeStream.WriteAsync(offsetBuffer, 0, offsetBuffer.Length, cancellationToken);
                await _writeStream.WriteAsync(data, 0, data.Length, cancellationToken);
                _lastFlushOffset = 8;
            }
            finally {
                _writeSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _writeStream.Dispose();
            _provider.Dispose();
            _offsetWriteStream.Dispose();
        }

    }
}
