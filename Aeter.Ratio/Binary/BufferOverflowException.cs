/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary
{
    public class BufferOverflowException : Exception
    {

        public BufferOverflowException(string message, Exception innerException) : base(message, innerException) { }
        public BufferOverflowException(string message) : base(message) { }

    }
}
