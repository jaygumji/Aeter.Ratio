/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Aeter.Ratio.Binary.Information;

namespace Aeter.Ratio.IO
{
    public interface IDataWriter
    {
        void Write(Byte value);
        void Write(Int16 value);
        void Write(Int32 value);
        void Write(Int64 value);
        void Write(UInt16 value);
        void Write(UInt32 value);
        void Write(UInt64 value);
        void Write(Boolean value);
        void Write(Single value);
        void Write(Double value);
        void Write(Decimal value);
        void Write(TimeSpan value);
        void Write(DateTime value);
        void Write(String value);
        void Write(Guid value);
        void Write(byte[] value);
    }
}
