/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    internal sealed class BinaryEntityStoreIndexMetadata
    {
        public const byte CurrentVersion = 1;

        public BinaryEntityStoreIndexMetadata(string path, string valueTypeName, BinaryEntityStoreIndexCapabilities capabilities, byte version)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentException.ThrowIfNullOrWhiteSpace(valueTypeName);

            Path = path;
            ValueTypeName = valueTypeName;
            Capabilities = capabilities;
            Version = version;
        }

        public string Path { get; }
        public string ValueTypeName { get; }
        public BinaryEntityStoreIndexCapabilities Capabilities { get; }
        public byte Version { get; }

        public Type ValueType => Type.GetType(ValueTypeName, throwOnError: true, ignoreCase: false)
                                  ?? throw new InvalidOperationException($"Unable to resolve type '{ValueTypeName}' for index '{Path}'.");

        public static BinaryEntityStoreIndexMetadata Create(string path, Type valueType, BinaryEntityStoreIndexCapabilities capabilities)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(valueType);

            var typeName = valueType.AssemblyQualifiedName ?? valueType.FullName ?? valueType.Name;
            return new BinaryEntityStoreIndexMetadata(path, typeName, capabilities, CurrentVersion);
        }
    }
}
