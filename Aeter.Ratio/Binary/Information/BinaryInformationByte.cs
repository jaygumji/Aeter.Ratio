/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.Converters;
using System;
namespace Aeter.Ratio.Binary.Information
{
    public class BinaryInformationByte : IBinaryInformation<Byte>
    {
        private readonly BinaryConverterByte _converter;

        public BinaryInformationByte()
        {
            _converter = new BinaryConverterByte();
        }

        public IBinaryConverter<Byte> Converter { get { return _converter; } }

        public bool IsFixedLength { get { return true; } }

        public int FixedLength { get { return 1; } }

        IBinaryConverter IBinaryInformation.Converter { get { return _converter; } }

        public int LengthOf(Byte value)
        {
            return FixedLength;
        }

        public int LengthOf(object value)
        {
            return FixedLength;
        }

    }
}
