using System.Runtime.CompilerServices;
using System.Threading;

namespace MonitorSlim.Monitors
{
    public struct ShortRecursiveMonitor
    {
        private int _count;
        private int _thId;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (_thId == id)
                return true;
            return Interlocked.CompareExchange(ref _thId, id, 0) == 0;
        }

        public bool Free => Volatile.Read(ref _thId) == 0;

        public bool Busy => !Free;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() {
            if (--_count == 0)
                Volatile.Write(ref _thId, 0);
        }
    }
}
