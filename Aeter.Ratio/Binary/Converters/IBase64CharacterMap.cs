/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary.Converters
{
    public interface IBase64CharacterMap
    {
        unsafe void MapLast(ref byte* s, ref byte* t, ref int padding);
        unsafe void MapTo(ref byte* s, ref byte* t);
    }
}