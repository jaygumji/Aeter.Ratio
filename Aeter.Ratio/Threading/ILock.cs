/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Aeter.Ratio.Threading
{
    public interface ILock
    {
        ILockHandle Enter();
        bool TryEnter();
        bool TryEnter(TimeSpan timeLimit);

        void Exit();
    }

    public interface ILock<TKey>
    {
        ILockHandle Enter(TKey key);
        bool TryEnter(TKey key);
        bool TryEnter(TKey key, TimeSpan timeLimit);

        void Exit(TKey key);
    }
}
