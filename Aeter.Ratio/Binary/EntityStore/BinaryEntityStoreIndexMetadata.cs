/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Describes the persisted characteristics of a binary entity store index.
    /// </summary>
    internal sealed class BinaryEntityStoreIndexMetadata
    {
        /// <summary>
        /// Current format version for serialized metadata entries.
        /// </summary>
        public const byte CurrentVersion = 1;

        /// <summary>
        /// Initializes metadata describing a persisted index.
        /// </summary>
        /// <param name="path">Entity path covered by the index.</param>
        /// <param name="valueTypeName">Assembly qualified type name that the index stores.</param>
        /// <param name="capabilities">Capabilities exposed by the index.</param>
        /// <param name="version">Serialized metadata version.</param>
        public BinaryEntityStoreIndexMetadata(string path, string valueTypeName, BinaryEntityStoreIndexCapabilities capabilities, byte version)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentException.ThrowIfNullOrWhiteSpace(valueTypeName);

            Path = path;
            ValueTypeName = valueTypeName;
            Capabilities = capabilities;
            Version = version;
        }

        /// <summary>
        /// Gets the entity path that the index tracks.
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// Gets the assembly qualified name of the value type.
        /// </summary>
        public string ValueTypeName { get; }
        /// <summary>
        /// Gets the capabilities encoded for this index.
        /// </summary>
        public BinaryEntityStoreIndexCapabilities Capabilities { get; }
        /// <summary>
        /// Gets the serialized metadata version.
        /// </summary>
        public byte Version { get; }

        /// <summary>
        /// Resolves the <see cref="Type"/> described by <see cref="ValueTypeName"/>.
        /// </summary>
        public Type ValueType => Type.GetType(ValueTypeName, throwOnError: true, ignoreCase: false)
                                  ?? throw new InvalidOperationException($"Unable to resolve type '{ValueTypeName}' for index '{Path}'.");

        /// <summary>
        /// Creates metadata for a new index by capturing the provided type and capabilities.
        /// </summary>
        /// <param name="path">Entity path that will be indexed.</param>
        /// <param name="valueType">Type of value stored.</param>
        /// <param name="capabilities">Capabilities supported by the index.</param>
        public static BinaryEntityStoreIndexMetadata Create(string path, Type valueType, BinaryEntityStoreIndexCapabilities capabilities)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(valueType);

            var typeName = valueType.AssemblyQualifiedName ?? valueType.FullName ?? valueType.Name;
            return new BinaryEntityStoreIndexMetadata(path, typeName, capabilities, CurrentVersion);
        }
    }
}
