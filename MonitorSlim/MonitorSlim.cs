using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSlim
{
    public struct ShortMonitor
    {
        private int _thId;

        public void Enter()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (_thId != id)
            {
                var spinner = new SpinWait();
                while (Interlocked.CompareExchange(ref _thId, id, 0) != 0)
                    spinner.SpinOnce();
            }
        }

        public bool TryEnter()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (_thId == id)
                return true;
            return Interlocked.CompareExchange(ref _thId, id, 0) == 0;
        }

        public bool Free => _thId == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _thId = 0;
    }
}
