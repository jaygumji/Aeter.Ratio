/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.IO
{
    internal class PooledMemoryStream : IWriteStream
    {

        private IStreamProvider _provider;
        private readonly MemoryStream _stream;

        public PooledMemoryStream(IStreamProvider provider, MemoryStream stream)
        {
            _provider = provider;
            _stream = stream;
        }

        public MemoryStream Stream { get { return _stream; } }

        public long Length { get { return _stream.Length; } }

        public long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var length = _stream.Read(buffer, offset, count);
            return Task.FromResult(length);
        }

        public void Dispose()
        {
            _provider.Return(this);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _stream.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        public void Flush() => _stream.Flush();

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            _stream.Flush();
            return Task.CompletedTask;
        }

        public void FlushForced()
        {
            _stream.Flush();
        }
    }
}
