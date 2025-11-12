/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using Aeter.Ratio.Binary;
using Aeter.Ratio.IO;

namespace Aeter.Ratio.Serialization.PackedBinary
{
    public class PackedDataReadVisitor(BinaryReadBuffer buffer) : IReadVisitor
    {
        private readonly BinaryDataReader reader = new(buffer);
        private UInt32 _nextIndex;
        private bool _endOfLevel;

        private static bool SkipDataIndex(uint dataIndex, uint index)
        {
            return 0 < dataIndex && dataIndex < index;
        }

        private bool MoveToIndex(UInt32 index)
        {
            if (_endOfLevel) return false;
            if (_nextIndex > index) return false;
            if (_nextIndex > 0) {
                var nextIndex = _nextIndex;
                _nextIndex = 0;
                if (nextIndex == index) return true;
            }
            UInt32 dataIndex;
            while (SkipDataIndex(dataIndex = reader.ReadZ(), index)) {
                var byteLength = reader.ReadByte();
                if (byteLength == BinaryZPacker.Null) continue;

                if (byteLength != BinaryZPacker.VariabelLength)
                    reader.Skip(byteLength);
                else {
                    var length = reader.ReadUInt32();
                    reader.Skip(length);
                }
            }
            if (dataIndex == 0) {
                _endOfLevel = true;
                return false;
            }
            if (dataIndex > index) {
                _nextIndex = dataIndex;
                return false;
            }
            return true;
        }

        public ValueState TryVisit(VisitArgs args)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index))
                return ValueState.NotFound;

            var byteLength = reader.ReadByte();
            if (byteLength == BinaryZPacker.Null) return ValueState.Null;
            if (byteLength != BinaryZPacker.VariabelLength)
                throw new UnexpectedLengthException(args, byteLength);

            if (args.IsRoot) {
                return ValueState.Found;
            }

            reader.Skip(4);

            return ValueState.Found;
        }

        public void Leave(VisitArgs args)
        {
            if (_endOfLevel) {
                _endOfLevel = false;
                return;
            }

            if (args.IsRoot) return;
            if (args.Type.IsCollection()) return;
            if (args.Type.IsDictionary()) return;

            MoveToIndex(UInt32.MaxValue);
            _endOfLevel = false;
        }

        public bool TryVisitValue(VisitArgs args, out byte? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            if (length != BinaryInformation.Byte.FixedLength)
                throw new UnexpectedLengthException(args, length);

            value = (Byte)buffer.ReadByte();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out short? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = (Int16)BinaryPV64Packer.UnpackS(buffer, length);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out int? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = (Int32)BinaryPV64Packer.UnpackS(buffer, length);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out long? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = BinaryPV64Packer.UnpackS(buffer, length);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out ushort? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = (UInt16)BinaryPV64Packer.UnpackU(buffer, length);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out uint? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = (UInt32)BinaryPV64Packer.UnpackU(buffer, length);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out ulong? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = BinaryPV64Packer.UnpackU(buffer, length);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out bool? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            if (length != BinaryInformation.Boolean.FixedLength)
                throw new UnexpectedLengthException(args, length);

            value = reader.ReadBoolean();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out float? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            if (length != BinaryInformation.Single.FixedLength)
                throw new UnexpectedLengthException(args, length);

            value = reader.ReadSingle();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out double? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            if (length != BinaryInformation.Double.FixedLength)
                throw new UnexpectedLengthException(args, length);

            value = reader.ReadDouble();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out decimal? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            if (length != BinaryInformation.Decimal.FixedLength)
                throw new UnexpectedLengthException(args, length);

            value = reader.ReadDecimal();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out TimeSpan? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = new TimeSpan(BinaryPV64Packer.UnpackS(buffer, length));
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out DateTime? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            value = new DateTime(BinaryPV64Packer.UnpackS(buffer, length), DateTimeKind.Utc).ToLocalTime();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out string? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }

            var lengthToRead = length == BinaryZPacker.VariabelLength ? reader.ReadV() : length;

            value = reader.ReadString(lengthToRead);
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out Guid? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }
            if (length != BinaryInformation.Guid.FixedLength)
                throw new UnexpectedLengthException(args, length);

            value = reader.ReadGuid();
            return true;
        }

        public bool TryVisitValue(VisitArgs args, out byte[]? value)
        {
            if (args.Index > 0 && !MoveToIndex(args.Index)) {
                value = null;
                return false;
            }
            var length = reader.ReadByte();
            if (length == BinaryZPacker.Null) {
                value = null;
                return true;
            }

            var lengthToRead = length == BinaryZPacker.VariabelLength ? reader.ReadV() : length;

            value = reader.ReadBlob(lengthToRead);
            return true;
        }
    }
}