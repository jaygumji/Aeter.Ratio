/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary
{
    public interface IBinaryConverter
    {
        object Convert(ReadOnlySpan<byte> value);
        byte[] Convert(object value);
        void Convert(object value, Span<byte> buffer);
        void Convert(object value, BinaryWriteBuffer writeBuffer);
    }

    public interface IBinaryConverter<T> : IBinaryConverter
    {
        new T Convert(ReadOnlySpan<byte> value);
        byte[] Convert(T value);
        void Convert(T value, Span<byte> buffer);
        void Convert(T value, BinaryWriteBuffer writeBuffer);
    }
}
