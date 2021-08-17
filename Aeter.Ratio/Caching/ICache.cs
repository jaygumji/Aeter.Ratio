/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Caching
{
    /// <summary>
    /// Contract of a cache
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Set the value given a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="content">The value</param>
        void Set(object key, object content);

        /// <summary>
        /// Set the value given a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="content">The value</param>
        /// <param name="policy">The policy that controls when the value expires</param>
        void Set(object key, object content, ICachePolicy policy);

        /// <summary>
        /// Try to set value given a key, not overwriting existing value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="contentGetter">A method that takes the key as input and expects the new value as output</param>
        /// <returns>The cached value</returns>
        /// <remarks>
        /// <para>If the cache already contains the value given the key, it will return that value.
        /// On the other hand if the cache does not contain the value given the key,
        /// it will set it by using the valueGetter function to retrieve the value value</para>
        /// </remarks>
        object TrySet(object key, Func<object, object> contentGetter);

        /// <summary>
        /// Try to set value given a key, not overwriting existing value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="contentGetter">A method that takes the key as input and expects the new value as output</param>
        /// <param name="policy">The policy that controls when the value expires</param>
        /// <returns>The cached value</returns>
        /// <remarks>
        /// <para>If the cache already contains the value given the key, it will return that value.
        /// On the other hand if the cache does not contain the value given the key,
        /// it will set it by using the valueGetter function to retrieve the value value</para>
        /// </remarks>
        object TrySet(object key, Func<object, object> contentGetter, ICachePolicy policy);

        /// <summary>
        /// Get the value given a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        /// <exception cref="CachedContentNotFoundException">Will be cast if no value exists that corresponds to the given key</exception>
        object Get(object key);

        /// <summary>
        /// Try to get the value given a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryGet(object key, out object content);
    }

    /// <summary>
    /// Contract of a cache
    /// </summary>
    public interface ICache<TKey, TContent> : ICache
    {
        /// <summary>
        /// Set the value given a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        void Set(TKey key, TContent value);

        /// <summary>
        /// Set the value given a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <param name="policy">The policy that controls when the value expires</param>
        void Set(TKey key, TContent value, ICachePolicy policy);
        
        /// <summary>
        /// Try to set value given a key, not overwriting existing value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="valueGetter">A method that takes the key as input and expects the new value as output</param>
        /// <returns>The cached value</returns>
        /// <remarks>
        /// <para>If the cache already contains the value given the key, it will return that value.
        /// On the other hand if the cache does not contain the value given the key,
        /// it will set it by using the valueGetter function to retrieve the value value</para>
        /// </remarks>
        TContent TrySet(TKey key, Func<TKey, TContent> valueGetter);

        /// <summary>
        /// Try to set value given a key, not overwriting existing value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="valueGetter">A method that takes the key as input and expects the new value as output</param>
        /// <param name="policy">The policy that controls when the value expires</param>
        /// <returns>The cached value</returns>
        /// <remarks>
        /// <para>If the cache already contains the value given the key, it will return that value.
        /// On the other hand if the cache does not contain the value given the key,
        /// it will set it by using the valueGetter function to retrieve the value value</para>
        /// </remarks>
        TContent TrySet(TKey key, Func<TKey, TContent> valueGetter, ICachePolicy policy);

        /// <summary>
        /// Get the value given a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        /// <exception cref="CachedContentNotFoundException">Will be cast if no value exists that corresponds to the given key</exception>
        TContent Get(TKey key);

        /// <summary>
        /// Try to get the value given a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryGet(TKey key, out TContent content);
    }
}