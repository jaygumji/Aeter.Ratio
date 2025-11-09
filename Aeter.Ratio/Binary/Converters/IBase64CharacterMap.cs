/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Binary.Converters
{
    public interface IBase64CharacterMap
    {
        void MapLast(System.ReadOnlySpan<byte> source, ref int sourceIndex, System.Span<byte> target, ref int targetIndex, ref int padding);
        void MapTo(System.ReadOnlySpan<byte> source, ref int sourceIndex, System.Span<byte> target, ref int targetIndex);
    }
}
