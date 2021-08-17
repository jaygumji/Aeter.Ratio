/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.IO
{
    public class WriteReservation
    {
        private readonly long _position;

        public WriteReservation(long position)
        {
            _position = position;
        }

        public long Position { get { return _position; } }
    }
}