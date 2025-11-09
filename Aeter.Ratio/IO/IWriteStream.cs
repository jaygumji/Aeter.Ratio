/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.IO
{
    public interface IWriteStream : IReadStream
    {
        void Write(byte[] buffer, int offset, int count);
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }
}
