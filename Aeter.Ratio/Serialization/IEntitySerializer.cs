/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using System;

namespace Aeter.Ratio.Serialization
{
    public interface IEntitySerializer
    {
        void Serialize(BinaryWriteBuffer buffer, object graph);
        object? Deserialize(Type type, BinaryReadBuffer buffer);
        void Serialize<T>(BinaryWriteBuffer buffer, T graph);
        T Deserialize<T>(BinaryReadBuffer buffer);
    }
}