using System.Runtime.CompilerServices;
using System.Threading;

namespace MonitorSlim.Monitors
{
    public struct ShortNonSpinnedMonitor
    {
        private int _lock;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter() => Interlocked.CompareExchange(ref _lock, 1, 0) == 0;

        public bool Free => _lock == 0;

        public bool Busy => !Free;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Volatile.Write(ref _lock, 0);
    }
}
