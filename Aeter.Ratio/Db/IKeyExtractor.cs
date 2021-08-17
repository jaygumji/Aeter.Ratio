/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Db
{
    /// <summary>
    /// A key extractor extracts the key from entities
    /// </summary>
    public interface IKeyExtractor
    {
        //byte[] ExtractBytes(object entity);

        /// <summary>Extracts the key as a value.</summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The key</returns>
        object ExtractValue(object entity);
    }
}