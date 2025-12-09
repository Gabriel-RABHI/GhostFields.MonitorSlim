using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MonitorSlim.Monitors
{
    /// <summary>
    /// Standalone lock primitive for narrow critical sections.
    /// </summary>
    /// <remarks>
    /// This synchronysation primitive is not recursive : the same thread cannot enter multiple times.
    /// </remarks>
    public struct ShortMonitor
    {
        private volatile int _lock;

        /// <summary>
        /// Acquires the lock. Blocks the current thread (spinning) until the lock is acquired.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            if (Interlocked.CompareExchange(ref _lock, 1, 0) == 0)
                return;
            EnterSlow();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnterSlow()
        {
            var spinner = new SpinWait();
            do
            {
                while (_lock != 0)
                    spinner.SpinOnce();
            } while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0);
        }

        /// <summary>
        /// Attempts to acquire the lock immediately.
        /// </summary>
        /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter() => Interlocked.CompareExchange(ref _lock, 1, 0) == 0;

        /// <summary>
        /// Gets a value indicating whether the lock is free.
        /// </summary>
        public bool Free => _lock == 0;

        /// <summary>
        /// Releases the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _lock = 0;
    }
}
