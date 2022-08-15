/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;

namespace Aeter.Ratio.Db
{
    public class EntityBinaryConverter
    {
        public EntityBinaryConverter(IKeyExtractor keyExtractor, IBinaryConverter entityConverter, IBinaryConverter keyConverter)
        {
            KeyExtractor = keyExtractor;
            EntityConverter = entityConverter;
            KeyConverter = keyConverter;
        }


        public IKeyExtractor KeyExtractor { get; }
        public IBinaryConverter EntityConverter { get; }
        public IBinaryConverter KeyConverter { get; }
    }
}