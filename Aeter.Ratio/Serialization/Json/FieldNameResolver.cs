/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Serialization.Json
{
    public class FieldNameResolver : IFieldNameResolver
    {
        public string Resolve(VisitArgs args)
        {
            if (!string.IsNullOrEmpty(args.Attributes.Name)) {
                return args.Attributes.Name;
            }
            return OnResolve(args);
        }

        protected virtual string OnResolve(VisitArgs args)
        {
            return args.Name;
        }
    }
}