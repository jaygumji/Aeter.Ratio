/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Serialization
{
    /// <summary>
    /// Changes the name of the field to a camel case syntax by
    /// making the first character to lower case.
    /// </summary>
    public class CamelCaseFieldNameResolver : FieldNameResolver
    {
        protected override string OnResolve(VisitArgs args)
        {
            if (args.Name == null) throw new ArgumentException("Name of supplied args is null");
            var res = args.Name.ToCharArray();
            res[0] = char.ToLowerInvariant(res[0]);
            return new string(res);
        }
    }
}