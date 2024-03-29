﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Aeter.Ratio.Serialization
{
    public static class GraphTravellerExtensions
    {
        public static IGraphTraveller<T> Get<T>(this IGraphTravellerProvider provider)
        {
            return (IGraphTraveller<T>)provider.Get(typeof(T));
        }
    }
}