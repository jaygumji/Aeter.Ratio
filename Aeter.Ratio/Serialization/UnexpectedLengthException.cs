/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization
{
    public class UnexpectedLengthException : Exception
    {
        public UnexpectedLengthException(VisitArgs args, uint length) : this(args.Name, args.Index, length) { }

        public UnexpectedLengthException(string? name, uint index, uint length) : base($"Unexpected length of {name}, index {index}, value was {length}")
        {
        }
    }
}