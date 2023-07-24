using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSlim
{
    public class ConcurrentList<T>
        where T : IComparable<T>
    {
        private ConcurrentListSlot<T>[] _slots;
        private int _writeIndex = -1;
        private int _count = 0;
        private MonitorSlim _lock;
        private Stack<int> _removed = new Stack<int>();

        public struct ConcurrentListSlot<T>
            where T : IComparable<T> {
            public bool Removed, Assigned;
            public T Value;
        }

        public ConcurrentList()
        {
            _slots = new ConcurrentListSlot<T>[32];
        }

        public int Count => _count;

        public int Add(T item)
        {
            _lock.Enter();
            try
            {
                if (_removed.TryPop(out var i))
                {
                    _slots[i].Value = item;
                    _slots[i].Removed = false;
                    _count++;
                    return i;
                }
                else
                {
                    _writeIndex++;
                _redo:
                    if (_writeIndex > _slots.Length - 1)
                    {
                        Array.Resize(ref _slots, _slots.Length * 2);
                        goto _redo;
                    }
                    else
                    {
                        _slots[_writeIndex].Value = item;
                        _slots[_writeIndex].Assigned = true;
                        _slots[_writeIndex].Removed = false;
                        _count++;
                        return _writeIndex;
                    }
                }
            }
            finally {
                _lock.Exit();
            }
        }

        public void Remove(T item)
        {
            _lock.Enter();
            try
            {
                for (int i = 0; i < _slots.Length; i++)
                    if (_slots[i].Value.Equals(item))
                    {
                        _slots[i].Removed = true;
                        _removed.Push(i);
                        _count--;
                    }
            }
            finally {
                _lock.Exit();
            }
        }

        public void RemoveIndex(int index)
        {
            _lock.Enter();
            try
            {
                if (index > _writeIndex)
                    throw new Exception($"Index {index} is out of range.");
                if (_slots[index].Removed)
                    throw new Exception($"Index {index} already removed.");
                _slots[index].Removed = true;
                _removed.Push(index);
                _count--;
            }
            finally {
                _lock.Exit();
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                ConcurrentListSlot<T>[] readed = _slots;
                foreach (var s in readed)
                {
                    if (!s.Assigned)
                        yield break;
                    if (!s.Removed)
                        yield return s.Value;
                }
            }
        }
    }
}
