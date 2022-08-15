/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class IntermediateGraphTravellerCollection
    {
        private readonly Dictionary<Type, IntermediateGraphTraveller> _pending;
        private readonly Dictionary<Type, IntermediateGraphTraveller> _completed;

        public IntermediateGraphTravellerCollection()
        {
            _pending = new Dictionary<Type, IntermediateGraphTraveller>();
            _completed = new Dictionary<Type, IntermediateGraphTraveller>();
        }

        public bool TryGet(Type type, [MaybeNullWhen(false)] out IntermediateGraphTraveller traveller)
        {
            return _pending.TryGetValue(type, out traveller) || _completed.TryGetValue(type, out traveller);
        }

        public void Complete()
        {
            while (_pending.Count > 0) {
                var pendings = _pending.Values.ToArray();
                _pending.Clear();

                foreach (var pending in pendings) {
                    pending.FinishContructing();
                    _completed.Add(pending.Type, pending);
                }
            }
        }

        public void Register(IntermediateGraphTraveller traveller)
        {
            if (_pending.ContainsKey(traveller.Type) || _completed.ContainsKey(traveller.Type)) {
                throw new ArgumentException($"Traveller for '{traveller.Type.FullName}' has already been registered");
            }
            _pending.Add(traveller.Type, traveller);
        }
    }
}