/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Threading
{
    /// <summary>
    /// A lock handle
    /// </summary>
    /// <seealso cref="Enigma.Threading.ILockHandle" />
    public class LockHandle : ILockHandle
    {
        private readonly ILock _lock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockHandle"/> class.
        /// </summary>
        /// <param name="lock">The lock.</param>
        public LockHandle(ILock @lock)
        {
            _lock = @lock;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            _lock.Exit();
        }
    }

    /// <summary>
    /// A lock handle
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Enigma.Threading.ILockHandle" />
    public class LockHandle<TKey> : ILockHandle
    {
        private readonly ILock<TKey> _lock;
        private readonly TKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockHandle{TKey}"/> class.
        /// </summary>
        /// <param name="lock">The lock.</param>
        /// <param name="key">The key.</param>
        public LockHandle(ILock<TKey> @lock, TKey key)
        {
            _lock = @lock;
            _key = key;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            _lock.Exit(_key);
        }
    }

}
