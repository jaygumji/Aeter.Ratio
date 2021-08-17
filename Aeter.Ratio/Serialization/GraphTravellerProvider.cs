/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Serialization
{
    public class GraphTravellerProvider : IGraphTravellerProvider
    {
        private readonly IGraphTravellerFactory _travellerFactory;
        private readonly Dictionary<Type, IGraphTraveller> _travellers;

        public GraphTravellerProvider(IGraphTravellerFactory travellerFactory)
        {
            _travellerFactory = travellerFactory;
            _travellers = new Dictionary<Type, IGraphTraveller>();
        }

        public IGraphTraveller Get(Type type)
        {
            if (_travellers.TryGetValue(type, out var traveller)) return traveller;
            lock (_travellers) {
                if (_travellers.TryGetValue(type, out traveller)) return traveller;

                traveller = _travellerFactory.Create(type);
                _travellers.Add(type, traveller);
                return traveller;
            }
        }
    }
}