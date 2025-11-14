/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Scheduling
{
    /// <summary>
    /// Represents a scheduled task callback.
    /// </summary>
    /// <param name="state">User defined state passed to the callback.</param>
    /// <param name="cancellationToken">Cancellation token for the execution.</param>
    public delegate Task ScheduledTaskDelegate(object? state, CancellationToken cancellationToken);
}
