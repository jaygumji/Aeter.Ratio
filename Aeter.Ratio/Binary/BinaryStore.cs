/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
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
        private readonly object _writeLock = new object();
        private readonly Lock<long> _writeOffsetLock = new Lock<long>();

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

            if (_writeStream.Length > start)
            {
                using (var readStream = _provider.AcquireReadStream())
                {
                    readStream.Seek(start, SeekOrigin.Begin);
                    var offsetBuffer = new byte[8];
                    readStream.Read(offsetBuffer, 0, offsetBuffer.Length);
                    _currentOffset = BitConverter.ToInt64(offsetBuffer, 0);
                }
            }
            else
            {
                _currentOffset = 8;
                var offsetBuffer = BitConverter.GetBytes(_currentOffset);
                _writeStream.Seek(start, SeekOrigin.Begin);
                _writeStream.Write(offsetBuffer, 0, offsetBuffer.Length);

                if (maxLength > 0)
                {
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

        private void UpdateOffset()
        {
            var offsetBuffer = BitConverter.GetBytes(_currentOffset);
            _offsetWriteStream.Write(offsetBuffer, 0, offsetBuffer.Length);
            _offsetWriteStream.Seek(-8, SeekOrigin.Current);
        }

        public async Task WriteAsync(long storeOffset, byte[] data)
        {
            var handle = await _writeOffsetLock.EnterAsync(storeOffset);
            try {
                using (var offsetWriteStream = _provider.AcquireWriteStream()) {
                    offsetWriteStream.Seek(storeOffset, SeekOrigin.Begin);
                    offsetWriteStream.Write(data, 0, data.Length);
                    if (_lastFlushOffset > storeOffset)
                        _lastFlushOffset = storeOffset - 1;
                }
            }
            finally {
                await handle.ReleaseAsync();
            }
        }

        public bool TryWrite(byte[] data, out long storeOffset)
        {
            lock (_writeLock)
            {
                if (!IsSpaceAvailable(data.Length))
                {
                    storeOffset = 0;
                    return false;
                }

                storeOffset = _currentOffset;
                _writeStream.Write(data, 0, data.Length);
                _writeStream.Flush();
                _currentOffset += data.Length;
                UpdateOffset();
                return true;
            }
        }

        private void EnsureFlushed(long offset)
        {
            if (offset < _lastFlushOffset) return;

            lock (_writeLock)
            {
                if (offset < _lastFlushOffset) return;

                _writeStream.FlushForced();
                _lastFlushOffset = _currentOffset;
            }
        }

        public byte[] ReadAll(out long offset)
        {
            offset = 0;
            EnsureFlushed(_currentOffset);

            if (_currentOffset <= _start + 8) return new byte[] { };

            var buffer = new byte[_currentOffset - _start - 8];
            using (var readStream = _provider.AcquireReadStream())
            {
                readStream.Seek(_start + 8, SeekOrigin.Begin);
                readStream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        public byte[] Read(long storeOffset, long length)
        {
            EnsureFlushed(storeOffset);

            var buffer = new byte[length];
            using (var readStream = _provider.AcquireReadStream())
            {
                readStream.Seek(storeOffset, SeekOrigin.Begin);
                readStream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        public void TruncateTo(byte[] data)
        {
            lock (_writeLock)
            {
                _currentOffset = 8 + data.Length;
                var offsetBuffer = BitConverter.GetBytes(_currentOffset);
                _writeStream.Seek(_start, SeekOrigin.Begin);
                _writeStream.Write(offsetBuffer, 0, offsetBuffer.Length);
                _writeStream.Write(data, 0, data.Length);
                _lastFlushOffset = 8;
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
