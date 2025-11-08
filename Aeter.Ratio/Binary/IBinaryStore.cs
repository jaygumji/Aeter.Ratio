/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public interface IBinaryStore
    {
        bool IsEmpty { get; }
        long Size { get; }

        bool IsSpaceAvailable(long length);

        Task WriteAsync(long storeOffset, byte[] data, CancellationToken cancellationToken);
        Task<(bool IsSuccessful, long Offset)> TryWriteAsync(byte[] data, CancellationToken cancellationToken);

        Task<(byte[] Data, long Offset)> ReadAllAsync(CancellationToken cancellationToken);
        Task<byte[]> ReadAsync(long storeOffset, long length, CancellationToken cancellationToken);

        Task TruncateToAsync(byte[] data, CancellationToken cancellationToken);
    }
}