/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio
{
    public class CodedException : Exception
    {
        public const int IoCAmbigiousConstructor = 1001;

        public CodedException()
        {
        }

        public CodedException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public CodedException(string message) : base(message)
        {
        }

        public CodedException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public CodedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CodedException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; }

    }
}
