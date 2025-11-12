/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.IO;

namespace Aeter.Ratio.Serialization
{
    public static class SerializerExtensions
    {
        public static void Serialize(this ISerializer serializer, Stream stream, object graph)
        {
            serializer.Serialize(BinaryStream.Passthrough(stream), graph);
        }
        public static object? Deserialize(this ISerializer serializer, Type type, Stream stream)
        {
            return serializer.Deserialize(type, BinaryStream.Passthrough(stream));
        }
        public static void Serialize<T>(this ISerializer serializer, Stream stream, T graph)
        {
            serializer.Serialize<T>(BinaryStream.Passthrough(stream), graph);
        }
        public static T Deserialize<T>(this ISerializer serializer, Stream stream)
        {
            return serializer.Deserialize<T>(BinaryStream.Passthrough(stream));
        }
    }
}