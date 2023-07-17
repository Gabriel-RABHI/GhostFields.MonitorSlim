using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSlim
{
    public struct MonitorSlim
    {
        private int _lock;

        public void Enter()
        {
            var spinner = new SpinWait();
            while (Interlocked.CompareExchange(ref _lock, 5, 0) != 0)
                spinner.SpinOnce();
        }

        public bool TryEnter()
            => Interlocked.CompareExchange(ref _lock, 5, 0) == 0;

        public bool Free => _lock == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Volatile.Write(ref _lock, 0);
    }
}
