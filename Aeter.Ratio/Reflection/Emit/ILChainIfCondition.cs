/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection.Emit;

namespace Aeter.Ratio.Reflection.Emit
{
    public class ILChainIfCondition
    {
        private readonly ILChainIf _chain;

        public ILChainIfCondition(ILGenerator il, ILGenerationHandler condition)
        {
            _chain = new ILChainIf(il) {
                Condition = condition
            };
        }

        public ILChainIfThen Then(ILGenerationHandler body)
        {
            _chain.Body = body;
            return new ILChainIfThen(_chain);
        }

    }
}