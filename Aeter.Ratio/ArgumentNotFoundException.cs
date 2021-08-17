/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio
{
    /// <summary>
    /// Exception that is thrown when an argument was not found
    /// </summary>
    public class ArgumentNotFoundException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="ArgumentNotFoundException"/>
        /// </summary>
        /// <param name="argumentName">The name of the argument</param>
        public ArgumentNotFoundException(string argumentName) : base(CreateMessage(argumentName))
        {
        }

        private static string CreateMessage(string argumentName)
        {
            return string.Format("The given name {0} was not found", argumentName);
        }
    }
}