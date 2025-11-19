/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Represents the fluent stage where an else block can be finalized.
    /// </summary>
    public class ILChainIfElse
    {
        private readonly ILChainIf _chain;

        /// <summary>
        /// Creates a wrapper around the ongoing chain.
        /// </summary>
        public ILChainIfElse(ILChainIf chain)
        {
            _chain = chain;
        }

        /// <summary>
        /// Completes the conditional structure.
        /// </summary>
        public void End()
        {
            _chain.End();
        }

    }
}
