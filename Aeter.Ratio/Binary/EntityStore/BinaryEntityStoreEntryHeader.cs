/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Metadata describing where a record is stored within the stream and how to interpret it.
    /// </summary>
    public readonly struct BinaryEntityStoreEntryHeader
    {
        internal BinaryEntityStoreEntryHeader(byte marker, int size, int headerLength, BinaryEntityStoreRecordMetadata metadata)
        {
            Marker = marker;
            Size = size;
            HeaderLength = headerLength;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the marker byte. Values other than 255 mean the record is active.
        /// </summary>
        public byte Marker { get; }
        /// <summary>
        /// Gets the total size of the record including header and payload.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Gets the length of the header portion.
        /// </summary>
        public int HeaderLength { get; }
        /// <summary>
        /// Gets the strongly typed metadata describing the entity.
        /// </summary>
        public BinaryEntityStoreRecordMetadata Metadata { get; }
        /// <summary>
        /// Gets the size of the payload portion.
        /// </summary>
        public int PayloadLength => Size - HeaderLength;
        /// <summary>
        /// Gets a value indicating whether the record is active.
        /// </summary>
        public bool IsInUse => Marker != 255;
    }
}
