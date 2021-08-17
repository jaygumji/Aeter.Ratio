/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Reflection;

namespace Aeter.Ratio.Serialization.Reflection.Emit
{
    public class PendingChildGraphTraveller
    {
        public FieldInfo Field { get; set; }
        public MethodInfo TravelWriteMethod { get; set; }
        public MethodInfo TravelReadMethod { get; set; }
    }
}