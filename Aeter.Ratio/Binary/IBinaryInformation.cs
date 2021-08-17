/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary
{
    public interface IBinaryInformation
    {
        bool IsFixedLength { get; }
        int FixedLength { get; }
        IBinaryConverter Converter { get; }

        int LengthOf(object value);
    }

    public interface IBinaryInformation<T> : IBinaryInformation
    {
        new IBinaryConverter<T> Converter { get; }
        
        int LengthOf(T value);
    }

}
