/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary
{
    public class BinaryStoreReadAllArgs(BinaryStore store, long offset, byte type, int size, object? state)
    {
        public BinaryStore Store { get; } = store;
        public long Offset { get; } = offset;
        public byte Type { get; } = type;
        public int Size { get; } = size;
        public object? State { get; } = state;
    }
}
