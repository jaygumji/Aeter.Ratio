/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Scheduling;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary.EntityStore
{
    public class BinaryEntityStoreIndexEngine
    {
        public static async Task<(BinaryEntityStoreIndexEngine IndexEngine, ScheduledTaskHandle InitHandle)> CreateAsync(BinaryEntityStoreFileSystem fileSystem,
            EntityEngineEvents events, BinaryEntityStoreLockManager lockManager, BinaryEntityStoreToc toc, Scheduler scheduler)
        {
            foreach (var index in fileSystem.Indexes) {

            }
            throw new System.NotImplementedException();
        }
    }
}
