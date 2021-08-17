/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.IO;
using System.Reflection;

namespace Aeter.Ratio.Test
{
    public static class Resource
    {
        public static Stream Get(string name)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return typeof(Resource).GetTypeInfo().Assembly.GetManifestResourceStream(name);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
