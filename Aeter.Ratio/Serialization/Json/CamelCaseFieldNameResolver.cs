/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization.Json
{
    /// <summary>
    /// Changes the name of the field to a camel case syntax by
    /// making the first character to lower case.
    /// </summary>
    public class CamelCaseFieldNameResolver : FieldNameResolver
    {
        protected override string OnResolve(VisitArgs args)
        {
            return char.ToLowerInvariant(args.Name[0]) + args.Name.Substring(1);
        }
    }
}