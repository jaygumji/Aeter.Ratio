/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.Converters;
using System;
namespace Aeter.Ratio.Binary.Information
{
    public class BinaryInformationString : IBinaryInformation<String>
    {
        private readonly BinaryConverterString _converter;

        public BinaryInformationString()
        {
            _converter = new BinaryConverterString();
        }

        public IBinaryConverter<String> Converter { get { return _converter; } }

        public bool IsFixedLength { get { return false; } }

        public int FixedLength { get { return -1; } }

        IBinaryConverter IBinaryInformation.Converter { get { return _converter; } }

        public int LengthOf(String value)
        {
            return value == null ? 0 : -1;
        }

        public int LengthOf(object value)
        {
            return LengthOf((String) value);
        }

    }
}
