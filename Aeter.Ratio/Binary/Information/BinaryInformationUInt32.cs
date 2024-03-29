﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.Converters;
using System;
namespace Aeter.Ratio.Binary.Information
{
    public class BinaryInformationUInt32 : IBinaryInformation<UInt32>
    {
        private readonly BinaryConverterUInt32 _converter;

        public BinaryInformationUInt32()
        {
            _converter = new BinaryConverterUInt32();
        }

        public IBinaryConverter<UInt32> Converter { get { return _converter; } }

        public bool IsFixedLength { get { return true; } }

        public int FixedLength { get { return 4; } }

        IBinaryConverter IBinaryInformation.Converter { get { return _converter; } }

        public int LengthOf(UInt32 value)
        {
            return FixedLength;
        }

        public int LengthOf(object value)
        {
            return FixedLength;
        }

    }
}
