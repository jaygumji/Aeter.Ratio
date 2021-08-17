/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;

namespace Aeter.Ratio.Serialization
{
    public interface IGenericSerializer
    {
        void Serialize(Stream stream, object graph);
        void Serialize<T>(Stream stream, T graph);

        object Deserialize(Stream stream, Type type);
        T Deserialize<T>(Stream stream);
    }
}