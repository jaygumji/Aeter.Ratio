/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization
{
    public class EncodingBinaryFormat : IEncodingBinaryFormat
    {
        public EncodingBinaryFormat(int minSize, int maxSize, int sizeIncrement, byte[] expandCodes, int markerOffset)
        {
            MinSize = minSize;
            MaxSize = maxSize;
            SizeIncrement = sizeIncrement;
            ExpandCodes = expandCodes;
            MarkerOffset = markerOffset;
        }

        public int MinSize { get; }
        public int MaxSize { get; }
        public int SizeIncrement { get; }
        public byte[] ExpandCodes { get; }
        public int MarkerOffset { get; }
    }
}