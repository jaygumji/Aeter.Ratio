/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Text;

namespace Aeter.Ratio.Binary.Converters
{
    public sealed class Base64EncodedCharacterSet
    {
        private const string BaselineSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public byte[] Chars { get; }
        public byte[] PaddingChar { get; }
        public int CharSize { get; }

        public Base64EncodedCharacterSet(Encoding encoding)
        {
            Chars = encoding.GetBytes(BaselineSet);
            PaddingChar = encoding.GetBytes("=");
            CharSize = Chars.Length / 64;
        }
    }
}