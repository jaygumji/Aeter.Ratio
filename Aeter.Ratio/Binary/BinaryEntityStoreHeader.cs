using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public class BinaryEntityStoreHeader(uint version = 1, ARID? serializerType = null)
    {
        private const int V1Size = 14;
        public int Size => V1Size;
        public uint Version { get; } = version;
        public ARID SerializerType { get; } = serializerType ?? Serialization.Bson.BsonSerializer.ARID;

        public async Task WriteToAsync(BinaryWriteBuffer buffer, CancellationToken cancellationToken = default)
        {
            var space = await buffer.WriteAsync(Size, cancellationToken);
            var span = space.Span;
            BinaryInformation.UInt32.Converter.Convert(Version, span[..4]);
            var serializerSpan = span.Slice(4, 10);
            var written = SerializerType.WriteTo(serializerSpan);
            if (written < serializerSpan.Length) {
                serializerSpan.Slice(written).Clear();
            }
        }

        public static async Task<BinaryEntityStoreHeader> ReadFromAsync(BinaryReadBuffer buffer, CancellationToken cancellationToken = default)
        {
            var space = await buffer.ReadAsync(V1Size, cancellationToken);
            var span = space.Span;
            var version = BinaryInformation.UInt32.Converter.Convert(span[..4]);
            var serializerBytes = span.Slice(4, 10);
            var terminator = serializerBytes.IndexOf((byte)0);
            if (terminator >= 0) {
                serializerBytes = serializerBytes[..terminator];
            }
            var serializerType = ARID.ReadFrom(serializerBytes);
            return new BinaryEntityStoreHeader(version, serializerType);
        }
    }
}
