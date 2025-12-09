using System.Runtime.CompilerServices;
using System.Threading;

namespace MonitorSlim.Monitors
{
    /// <summary>
    /// Standalone recursive lock primitive for narrow critical sections.
    /// </summary>
    public struct ShortRecursiveMonitor
    {
        private int _count;
        private int _thId;

        /// <summary>
        /// Acquires the lock. Blocks the current thread (spinning) until the lock is acquired.
        /// </summary>
        /// <remarks>
        /// If the lock is already held by the current thread, the recursion count is incremented.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (Volatile.Read(ref _thId) != id)
            {
                if (Interlocked.CompareExchange(ref _thId, id, 0) != 0)
                {
                    var spinner = new SpinWait();
                    while (Interlocked.CompareExchange(ref _thId, id, 0) != 0)
                        spinner.SpinOnce();
                }
            }
            _count++;
        }

        /// <summary>
        /// Attempts to acquire the lock immediately.
        /// </summary>
        /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (_thId == id)
                return true;
            return Interlocked.CompareExchange(ref _thId, id, 0) == 0;
        }

        /// <summary>
        /// Gets a value indicating whether the lock is free.
        /// </summary>
        public bool Free => Volatile.Read(ref _thId) == 0;

        /// <summary>
        /// Gets a value indicating whether the lock is currently held.
        /// </summary>
        public bool Busy => !Free;

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <remarks>
        /// Decrements the recursion count. The lock is released only when the count reaches zero.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() {
            if (--_count == 0)
                Volatile.Write(ref _thId, 0);
        }
    }
}
