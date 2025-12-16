using System.Runtime.CompilerServices;

namespace MonitorSlim.Monitors
{
    /// <summary>
    /// Standalone lock primitive for narrow critical sections using a busy-wait loop.
    /// </summary>
    /// <remarks>
    /// This synchronization primitive is not recursive.
    /// </remarks>
    public struct ShortNonSpinnedMonitor
    {
        private int _lock;

        /// <summary>
        /// Acquires the lock. Blocks the current thread (busy-waiting) until the lock is acquired.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            if (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
            {
                while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                {

                }
            }
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
        /// Gets a value indicating whether the lock is currently held.
        /// </summary>
        public bool Busy => !Free;

        /// <summary>
        /// Releases the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Volatile.Write(ref _lock, 0);
    }
}
