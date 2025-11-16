using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    public class BinaryEntityStoreTocEntry(long offset, int size, bool isFree, Guid key)
    {
        public long Offset { get; } = offset;
        public int Size { get; } = size;
        public bool IsFree { get; } = isFree;
        public Guid Key { get; } = key;
    }
}
