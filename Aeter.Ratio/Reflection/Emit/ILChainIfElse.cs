/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Reflection.Emit
{
    public class ILChainIfElse
    {
        private readonly ILChainIf _chain;

        public ILChainIfElse(ILChainIf chain)
        {
            _chain = chain;
        }

        public void End()
        {
            _chain.End();
        }

    }
}