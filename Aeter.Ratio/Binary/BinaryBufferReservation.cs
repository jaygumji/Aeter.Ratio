/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary
{
    public class BinaryBufferReservation
    {
        public int Position { get; }
        public int Size { get; }

        public BinaryBufferReservation(int position, int size)
        {
            Position = position;
            Size = size;
        }
    }
}