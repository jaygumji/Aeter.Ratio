/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio
{
    /// <summary>
    /// A bag of objects
    /// </summary>
    public class ObjectBag : IReadOnlyObjectBag
    {

        /// <summary>
        /// An empty bag
        /// </summary>
        public static IReadOnlyObjectBag Empty { get; } = new ObjectBag();


        private readonly Dictionary<string, object> _values;

        /// <summary>
        /// Creates a new instance of <see cref="ObjectBag"/>
        /// </summary>
        public ObjectBag()
        {
            _values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Sets a value in the list
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <param name="value">The value of the argument</param>
        public void Set(string name, object value)
        {
            _values[name] = value;
        }

        /// <summary>
        /// Gets a value in the list
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <returns>The argument value</returns>
        /// <exception cref="ArgumentNotFoundException">Thrown when the argument with the given name was not found</exception>
        public object Get(string name)
        {
            if (!_values.TryGetValue(name, out var value))
                throw new ArgumentNotFoundException(name);

            return value;
        }

        /// <inheritdoc />
        public bool TryGetValue(string name, [MaybeNullWhen(false)] out object value)
        {
            return _values.TryGetValue(name, out value);
        }

        /// <summary>
        /// Sets a value in the list
        /// </summary>
        /// <typeparam name="T">The type of the argument value</typeparam>
        /// <param name="value">The value of the argument</param>
        public void Set<T>(T value)
        {
            var name = typeof (T).FullName!;
            _values[name] = value!;
        }

        /// <inheritdoc />
        public T Get<T>()
        {
            var name = typeof(T).FullName!;
            return (T) Get(name);
        }

        /// <inheritdoc />
        public bool TryGetValue<T>([MaybeNullWhen(false)] out T value)
        {
            var name = typeof(T).FullName!;

            if (!_values.TryGetValue(name, out var untypedValue)) {
                value = default;
                return false;
            }

            value = (T) untypedValue;
            return true;
        }

    }
}
