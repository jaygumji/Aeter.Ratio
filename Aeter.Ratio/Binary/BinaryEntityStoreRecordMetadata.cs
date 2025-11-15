/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers.Binary;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// Versioned metadata stored in each record header.
    /// </summary>
    public readonly struct BinaryEntityStoreRecordMetadata
    {
        private const int V1Length = 20;
        public const byte CurrentMetadataVersion = 1;

        public BinaryEntityStoreRecordMetadata(Guid key, uint version, byte metadataVersion = CurrentMetadataVersion)
        {
            Key = key;
            Version = version;
            MetadataVersion = metadataVersion;
        }

        /// <summary>
        /// Gets the entity key.
        /// </summary>
        public Guid Key { get; }
        /// <summary>
        /// Gets the entity version number.
        /// </summary>
        public uint Version { get; }
        /// <summary>
        /// Gets the metadata format version.
        /// </summary>
        public byte MetadataVersion { get; }

        internal int GetSerializedLength()
            => MetadataVersion switch {
                CurrentMetadataVersion => V1Length,
                _ => throw new NotSupportedException($"Metadata version {MetadataVersion} is not supported.")
            };

        internal void WriteTo(Span<byte> destination)
        {
            var length = GetSerializedLength();
            if (destination.Length < length) {
                throw new ArgumentException("Insufficient destination span for metadata.", nameof(destination));
            }

            switch (MetadataVersion) {
                case CurrentMetadataVersion:
                    BinaryPrimitives.WriteUInt32LittleEndian(destination[..4], Version);
                    if (!Key.TryWriteBytes(destination.Slice(4, 16))) {
                        throw new InvalidOperationException("Unable to write entity key to metadata block.");
                    }
                    break;
                default:
                    throw new NotSupportedException($"Metadata version {MetadataVersion} is not supported.");
            }
        }

        internal static BinaryEntityStoreRecordMetadata ReadFrom(byte metadataVersion, ReadOnlySpan<byte> source)
        {
            return metadataVersion switch {
                CurrentMetadataVersion => ReadV1(source),
                _ => throw new NotSupportedException($"Metadata version {metadataVersion} is not supported.")
            };
        }

        private static BinaryEntityStoreRecordMetadata ReadV1(ReadOnlySpan<byte> source)
        {
            if (source.Length < V1Length) {
                throw new ArgumentException("Metadata span too small for v1 layout.", nameof(source));
            }

            var version = BinaryPrimitives.ReadUInt32LittleEndian(source[..4]);
            var key = new Guid(source.Slice(4, 16));
            return new BinaryEntityStoreRecordMetadata(key, version);
        }
    }
}
