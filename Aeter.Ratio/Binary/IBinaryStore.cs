/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary
{
    public interface IBinaryStore
    {
        bool IsEmpty { get; }
        long Size { get; }

        bool IsSpaceAvailable(long length);

        void Write(long storeOffset, byte[] data);
        bool TryWrite(byte[] data, out long storeOffset);

        byte[] ReadAll(out long offset);
        byte[] Read(long storeOffset, long length);

        void TruncateTo(byte[] data);
    }
}