using System.Runtime.CompilerServices;
using System.Threading;

namespace MonitorSlim.Monitors
{
    /// <summary>
    /// Standalone reader-writer lock primitive for narrow critical sections.
    /// </summary>
    /// <remarks>
    /// Supports multiple concurrent readers and exclusive writers.
    /// </remarks>
    public struct ShortReadWriteMonitor
    {
        private const int _maxReaders = int.MaxValue / 2;

        private int _count;

        /// <summary>
        /// Acquires the lock in read mode. Blocks if a writer holds the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead()
        {
            if (Interlocked.Increment(ref _count) < 0)
            {
                Interlocked.Decrement(ref _count);
                var spinner = new SpinWait();
                while (Interlocked.Increment(ref _count) < 0)
                {
                    Interlocked.Decrement(ref _count);
                    spinner.SpinOnce();
                }
            }
        }

        /// <summary>
        /// Acquires the lock in write mode. Blocks if any readers or writers hold the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite()
        {
            if (Interlocked.CompareExchange(ref _count, -1024, 0) != 0)
            {
                var spinner = new SpinWait();
                while (Interlocked.CompareExchange(ref _count, -1024, 0) != 0)
                {
                    spinner.SpinOnce();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the lock is completely free (no readers or writers).
        /// </summary>
        public bool ReadFree => _count == 0;

        /// <summary>
        /// Gets a value indicating whether the lock is completely free (no readers or writers).
        /// </summary>
        public bool WriteFree => _count == 0;

        /// <summary>
        /// Releases the read lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead() => Interlocked.Decrement(ref _count);

        /// <summary>
        /// Releases the write lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => Interlocked.Add(ref _count, 1024);
    }
}
