/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.Converters;
using System;
namespace Aeter.Ratio.Binary.Information
{
    public class BinaryInformationDecimal : IBinaryInformation<Decimal>
    {
        private readonly BinaryConverterDecimal _converter;

        public BinaryInformationDecimal()
        {
            _converter = new BinaryConverterDecimal();
        }

        public IBinaryConverter<Decimal> Converter { get { return _converter; } }

        public bool IsFixedLength { get { return true; } }

        public int FixedLength { get { return 16; } }

        IBinaryConverter IBinaryInformation.Converter { get { return _converter; } }

        public int LengthOf(Decimal value)
        {
            return FixedLength;
        }

        public int LengthOf(object value)
        {
            return FixedLength;
        }

    }
}
