/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;

namespace Aeter.Ratio.Serialization
{
    public interface ISerializer
    {
        void Serialize(Stream stream, object graph);
        object? Deserialize(Type type, Stream stream);
    }

    public interface ITypedSerializer
    {
        void Serialize(Stream stream, object graph);
        object? Deserialize(Stream stream);
    }

    public interface ITypedSerializer<T> : ITypedSerializer
    {
        void Serialize(Stream stream, T graph);
        new T Deserialize(Stream stream);
    }
}