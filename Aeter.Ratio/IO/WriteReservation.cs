/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.IO
{
    /// <summary>
    /// Represents a reserved region within a stream that can be filled later.
    /// </summary>
    public class WriteReservation
    {
        private readonly long _position;

        /// <summary>
        /// Initializes a reservation at the specified absolute position.
        /// </summary>
        /// <param name="position">Absolute position of the reservation.</param>
        public WriteReservation(long position)
        {
            _position = position;
        }

        /// <summary>
        /// Gets the absolute position associated with this reservation.
        /// </summary>
        public long Position { get { return _position; } }
    }
}
