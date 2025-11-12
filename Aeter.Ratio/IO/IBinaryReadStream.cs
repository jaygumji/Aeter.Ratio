/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.IO
{
    public interface IBinaryReadStream : IBinaryStream
    {
        int Read(long offset, Span<byte> buffer);
        ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
