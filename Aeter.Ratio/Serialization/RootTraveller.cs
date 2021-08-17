/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization
{
    public class RootTraveller
    {
        public IGraphTraveller Traveller { get; }
        public LevelType Level { get; }

        public RootTraveller(IGraphTraveller traveller, LevelType level)
        {
            Traveller = traveller;
            Level = level;
        }
    }
}