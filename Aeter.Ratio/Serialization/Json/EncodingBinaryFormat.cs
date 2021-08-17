/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization.Json
{
    public class EncodingBinaryFormat : IEncodingBinaryFormat
    {
        public int MinSize { get; set; }
        public int MaxSize { get; set; }
        public int SizeIncrement { get; set; }
        public byte[] ExpandCodes { get; set; }
        public int MarkerOffset { get; set; }
    }
}