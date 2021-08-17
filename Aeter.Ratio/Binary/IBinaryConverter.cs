/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary
{
    public interface IBinaryConverter
    {
        object Convert(byte[] value);
        object Convert(byte[] value, int startIndex);
        object Convert(byte[] value, int startIndex, int length);
        byte[] Convert(object value);
        void Convert(object value, byte[] buffer);
        void Convert(object value, byte[] buffer, int offset);
        void Convert(object value, BinaryWriteBuffer writeBuffer);
    }

    public interface IBinaryConverter<T> : IBinaryConverter
    {
        new T Convert(byte[] value);
        new T Convert(byte[] value, int startIndex);
        new T Convert(byte[] value, int startIndex, int length);
        byte[] Convert(T value);
        void Convert(T value, byte[] buffer);
        void Convert(T value, byte[] buffer, int offset);
        void Convert(T value, BinaryWriteBuffer writeBuffer);
    }
}
