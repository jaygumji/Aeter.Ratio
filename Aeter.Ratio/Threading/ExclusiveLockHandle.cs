/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aeter.Ratio.Threading
{
    class ExclusiveLockHandle : ILockHandle
    {
        private readonly ExclusiveLock _lock;

        public ExclusiveLockHandle(ExclusiveLock @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            _lock.ExitExclusive();
        }
    }
}
