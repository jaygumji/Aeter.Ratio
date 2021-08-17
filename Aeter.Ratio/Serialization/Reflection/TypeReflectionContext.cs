/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aeter.Ratio.Reflection;

namespace Aeter.Ratio.Serialization.Reflection
{
    public class TypeReflectionContext
    {

        private static readonly ConcurrentDictionary<Type, TypeReflectionContext> Cache = new ConcurrentDictionary<Type, TypeReflectionContext>();

        public static TypeReflectionContext Get(Type type, ITypeProvider provider)
        {
            return Cache.GetOrAdd(type, t => new TypeReflectionContext(t, provider));
        }

        private readonly Type _type;
        private readonly Lazy<IList<PropertyReflectionContext>> _properties;
        private readonly ExtendedType _extended;

        private TypeReflectionContext(Type type, ITypeProvider provider)
        {
            _type = type;
            _extended = provider.Extend(type);
            _properties = new Lazy<IList<PropertyReflectionContext>>(() => ParseProperties(type, provider));
        }

        private static IList<PropertyReflectionContext> ParseProperties(Type type, ITypeProvider provider)
        {
            var properties = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite)
                .Select(p => new PropertyReflectionContext(p, provider))
                .ToList();

            uint index = 0;
            foreach (var property in properties)
                property.Index = ++index;

            return properties;
        }

        public Type Type { get { return _type; } }
        public ExtendedType Extended { get { return _extended; } }
        public IEnumerable<PropertyReflectionContext> SerializableProperties { get { return _properties.Value; } }

    }

    public class PropertyReflectionContext
    {
        private readonly PropertyInfo _property;
        private readonly TypeReflectionContext _propertyTypeContext;

        public PropertyReflectionContext(PropertyInfo property, ITypeProvider provider)
        {
            _property = property;
            _propertyTypeContext = TypeReflectionContext.Get(property.PropertyType, provider);
        }

        public TypeReflectionContext PropertyTypeContext
        {
            get { return _propertyTypeContext; }
        }

        public PropertyInfo Property
        {
            get { return _property; }
        }

        public uint Index { get; internal set; }
    }
}
