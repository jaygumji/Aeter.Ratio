/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;

namespace Aeter.Ratio.Serialization
{
    public interface ISerializer
    {
        void Serialize(IBinaryWriteStream stream, object graph);
        object? Deserialize(Type type, IBinaryReadStream stream);
        void Serialize<T>(IBinaryWriteStream stream, T graph);
        T Deserialize<T>(IBinaryReadStream stream);
    }
}