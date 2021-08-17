/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.IO
{
    public interface IDataReader
    {
        Byte ReadByte();
        Int16 ReadInt16();
        Int32 ReadInt32();
        Int64 ReadInt64();
        UInt16 ReadUInt16();
        UInt32 ReadUInt32();
        UInt64 ReadUInt64();
        Boolean ReadBoolean();
        Single ReadSingle();
        Double ReadDouble();
        Decimal ReadDecimal();
        TimeSpan ReadTimeSpan();
        DateTime ReadDateTime();
        String ReadString();
        String ReadString(uint length);
        Guid ReadGuid();
        byte[] ReadBlob();
        byte[] ReadBlob(uint length);
    }
}