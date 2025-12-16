using System.Runtime.CompilerServices;

namespace MonitorSlim.Monitors
{
    /// <summary>
    /// Standalone lock primitive that accept a limited entrancy count.
    /// <remarks>
    /// This synchronysation primitive is not recursive : the same thread cannot enter multiple times.
    /// </remarks>
    /// </summary>
    public struct ShortCountMonitor
    {
        private int _count;
        private volatile int _max;

        /// <summary>
        /// Construct the primitive by specifying the number of enters allowed.
        /// </summary>
        /// <param name="max">Number of enters allowed</param>
        public ShortCountMonitor(int max)
        {
            _count = 0;
            _max = max;
        }

        /// <summary>
        /// Enter the lock if the entering count is not reached.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            if (Interlocked.Increment(ref _count) > _max)
            {
                Interlocked.Decrement(ref _count);
                var spinner = new SpinWait();
                while (Interlocked.Increment(ref _count) > _max)
                {
                    Interlocked.Decrement(ref _count);
                    spinner.SpinOnce();
                }
            }
        }

        /// <summary>
        /// Return true if the maximum entering count is not reached.
        /// </summary>
        public bool Free => _count < _max;

        /// <summary>
        /// Release the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Decrement(ref _count);
    }
}
