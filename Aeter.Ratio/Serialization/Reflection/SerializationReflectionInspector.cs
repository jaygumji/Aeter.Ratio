/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class SerializationReflectionInspector
    {

        public bool CanBeSerialized(Type type, PropertyInfo property)
        {
            if (!property.CanWrite) return false;

            var args = new PropertyValidArgs(type, property);
            IsPropertyValid(args);
            return args.IsValid;
        }

        public bool CanBeDeserialized(Type type, PropertyInfo property)
        {
            if (!property.CanRead) return false;

            var args = new PropertyValidArgs(type, property);
            IsPropertyValid(args);
            return args.IsValid;
        }

        public bool CanBeSerialized(Type type)
        {
            var args = new TypeValidArgs(type);
            IsTypeValid(args);
            return args.IsValid;
        }

        public bool CanBeDeserialized(Type type)
        {
            var args = new TypeValidArgs(type);
            IsTypeValid(args);
            return args.IsValid;
        }

        public SerializationMetadata AcquirePropertyMetadata(Type type, PropertyInfo property, ref uint nextIndex)
        {
            var args = new AcquirePropertyMetadataArgs(type, property);
            OnAcquirePropertyMetadata(args);

            var index = args.Index ?? nextIndex;
            nextIndex = index + 1;
            var metadata = new SerializationMetadata(index);
            return metadata;
        }

        protected virtual void IsPropertyValid(PropertyValidArgs args)
        {
            args.IsValid = args.Property.GetCustomAttribute<IgnoreAttribute>() == null;
        }

        protected virtual void IsTypeValid(TypeValidArgs args) { }

        protected virtual void OnAcquirePropertyMetadata(AcquirePropertyMetadataArgs args)
        {
            args.Index = args.Property.GetCustomAttribute<IndexAttribute>()?.Index;
        }

    }
}