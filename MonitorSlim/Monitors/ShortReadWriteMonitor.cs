using System.Runtime.CompilerServices;
using System.Threading;

namespace MonitorSlim.Monitors
{
    public struct ShortReadWriteMonitor
    {
        private const int _maxReaders = int.MaxValue / 2;

        private int _count;

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

        public bool ReadFree => _count == 0;

        public bool WriteFree => _count == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead() => Interlocked.Decrement(ref _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => Interlocked.Add(ref _count, 1024);
    }
}
