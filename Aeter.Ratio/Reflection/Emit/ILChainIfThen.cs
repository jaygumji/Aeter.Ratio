/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Reflection.Emit
{
    /// <summary>
    /// Represents the fluent stage where the conditional already has a body defined.
    /// </summary>
    public class ILChainIfThen
    {
        private readonly ILChainIf _chain;

        /// <summary>
        /// Creates a wrapper for the provided chain.
        /// </summary>
        public ILChainIfThen(ILChainIf chain)
        {
            _chain = chain;
        }

        /// <summary>
        /// Adds an else branch to the chain.
        /// </summary>
        public ILChainIfElse Else(ILGenerationHandler body)
        {
            _chain.ElseBody = body;
            return new ILChainIfElse(_chain);
        }

        /// <summary>
        /// Emits the conditional body without an else block.
        /// </summary>
        public void End()
        {
            _chain.End();
        }

    }
}
