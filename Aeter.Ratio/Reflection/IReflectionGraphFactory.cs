/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Reflection
{
    /// <summary>
    /// Interface IReflectionGraphFactory
    /// </summary>
    public interface IReflectionGraphFactory
    {
        /// <summary>
        /// Tries to get the property visitor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="visitor">The visitor.</param>
        /// <returns><c>true</c> if visitor available, <c>false</c> otherwise.</returns>
        bool TryCreatePropertyVisitor(Type type, out IReflectionGraphPropertyVisitor visitor);
    }
}
