namespace MonitorSlim
{
    public class ConcurrentQueueSlim<T>
    {
        public const int SEG_SIZE = 4096;

        private class ConcurrentManagedQueueSegment<T>
        {
            public ushort _head, _tail;
            public T[] _map = new T[SEG_SIZE];
            public ConcurrentManagedQueueSegment<T> _next;
        }

        private ConcurrentManagedQueueSegment<T> _currentWrite;
        private ConcurrentManagedQueueSegment<T> _currentRead;
        private int _thId = 0;

        public ConcurrentQueueSlim()
        {
            _currentWrite = _currentRead = new ConcurrentManagedQueueSegment<T>();
        }

        public void Enqueue(T v)
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (_thId != id)
            {
                var spinner = new SpinWait();
                while (Interlocked.CompareExchange(ref _thId, id, 0) != 0)
                    spinner.SpinOnce();
            }
            /********/
            if (_currentWrite._head < SEG_SIZE)
                _currentWrite._map[_currentWrite._head++] = v;
            else
            {
                _currentWrite = _currentWrite._next = new ConcurrentManagedQueueSegment<T>();
                _currentWrite._map[_currentWrite._head++] = v;
            }
            /********/
            _thId = 0;
        }

        public bool TryDequeue(out T r)
        {
            if (_currentRead._head > _currentRead._tail || _currentRead != _currentWrite)
            {
                var id = Thread.CurrentThread.ManagedThreadId;
                if (_thId != id)
                {
                    var spinner = new SpinWait();
                    while (Interlocked.CompareExchange(ref _thId, id, 0) != 0)
                        spinner.SpinOnce();
                }
                /********/
            _redo:
                if (_currentRead._tail < _currentRead._head)
                {
                    r = _currentRead._map[_currentRead._tail++];
                    _thId = 0;
                    return true;
                }
                else
                {
                    if (_currentRead._tail == SEG_SIZE && _currentRead._next != null)
                    {
                        _currentRead = _currentRead._next;
                        goto _redo;
                    }
                }
                /********/
                _thId = 0;
            }
            r = default;
            return false;
        }
    }
}