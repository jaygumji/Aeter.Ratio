/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.Converters;
using System;
namespace Aeter.Ratio.Binary.Information
{
    public class BinaryInformationSByte : IBinaryInformation<SByte>
    {
        private readonly BinaryConverterSByte _converter;

        public BinaryInformationSByte()
        {
            _converter = new BinaryConverterSByte();
        }

        public IBinaryConverter<SByte> Converter { get { return _converter; } }

        public bool IsFixedLength { get { return true; } }

        public int FixedLength { get { return 1; } }

        IBinaryConverter IBinaryInformation.Converter { get { return _converter; } }

        public int LengthOf(SByte value)
        {
            return FixedLength;
        }

        public int LengthOf(object value)
        {
            return FixedLength;
        }

    }
}
