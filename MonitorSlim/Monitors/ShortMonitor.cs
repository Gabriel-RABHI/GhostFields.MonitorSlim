#define OPTIMIZED

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MonitorSlim.Monitors
{

#if OPTIMIZED
    public struct ShortMonitor
    {
        private volatile int _lock;

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
                while (_lock != 0)  // Fast volatile read
                    spinner.SpinOnce();
            } while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter() => Interlocked.CompareExchange(ref _lock, 1, 0) == 0;

        public bool Free => _lock == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _lock = 0;  // Volatile write
    }
#else

    /// <summary>
    /// Standalone lock primitive for narrow critical sections.
    /// </summary>
    /// <remarks>
    /// This synchronysation primitive is not recursive : the same thread cannot enter multiple times.
    /// </remarks>
    public unsafe struct ShortMonitor
    {
        private int _lock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            if (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
            {
                var spinner = new SpinWait();
                while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                    spinner.SpinOnce();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter() => Interlocked.CompareExchange(ref _lock, 1, 0) == 0;

        public bool Free => _lock == 0;

        public bool Busy => !Free;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Volatile.Write(ref _lock, 0);
    }
#endif
}
