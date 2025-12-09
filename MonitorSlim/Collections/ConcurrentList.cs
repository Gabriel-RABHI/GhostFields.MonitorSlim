#define ACURRATE_VALUES_NUMBER

using MonitorSlim.Monitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSlim.Collections
{
    /// <summary>
    /// Naive, Fast concurrent list : on one side, the writers are serialized by a short lock,
    /// but on the enumerators side there is a lock free yield return loop on the entries.
    /// It make it thread safe.
    /// Drawback : the memory used by the structure is the one used at the peak count item.
    /// In other words, if, during the lifetime of the instance of this class, you insert
    /// 1 Million items, the memory used by this instance will never be shrinked to
    /// use less memory.
    /// Caution : this can be a advantage for all subsequent variation of
    /// item count, because the memory is preallocated. But if you have large variations of item
    /// count, then it can use large memory space unecessary.
    /// So, this class fit for use case where you have average max item count relativelly stable
    /// with large variations : in that case, performance will be good.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentList<T>
        where T : IComparable<T>
    {
        private ConcurrentListSlot<T>[] _slots = new ConcurrentListSlot<T>[32];
        private int _writeIndex = -1;
        private int _count = 0;
        private ShortMonitor _lock;
        private Stack<int> _removed = new Stack<int>();

        public struct ConcurrentListSlot<T>
            where T : IComparable<T> {
            public bool Removed, Assigned;
            public T Value;
        }

        /// <summary>
        /// Item count.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Add a new item to the list and return the private, internal index
        /// that can be used to remove the item in an O(1) delay.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The internal index of the item, used to remove it if necessary</returns>
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
                    if (_writeIndex > _slots.Length - 1)
                        Array.Resize(ref _slots, _slots.Length * 2);
                    _slots[_writeIndex].Value = item;
                    _slots[_writeIndex].Assigned = true;
                    _slots[_writeIndex].Removed = false;
                    _count++;
                    return _writeIndex;
                }
            }
            finally {
                _lock.Exit();
            }
        }

        /// <summary>
        /// Remove all item that are Equal to the given one.
        /// </summary>
        /// <param name="item">The value to remove</param>
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

        /// <summary>
        /// Fast O(1) removing of an item.
        /// </summary>
        /// <param name="index">The internal index.</param>
        /// <exception cref="ArgumentException">Wrong parameters : overflow of already removed item index.</exception>
        public void RemoveIndex(int index)
        {
            _lock.Enter();
            try
            {
                if (index > _writeIndex || index < 0)
                    throw new ArgumentException($"Index {index} is out of range.");
                if (_slots[index].Removed)
                    throw new ArgumentException($"Index {index} already removed.");
                _slots[index].Removed = true;
                _removed.Push(index);
                _count--;
            }
            finally {
                _lock.Exit();
            }
        }

#if !ACURRATE_VALUES_NUMBER
        /// <summary>
        /// A lock free item enumerable.
        /// </summary>
        public IEnumerable<T> Values
        {
            get
            {
                ConcurrentListSlot<T>[] readed = _slots;
                var n = 0;
                var count = _count;
                foreach (var s in readed)
                {
                    if (!s.Assigned || n > count)
                        yield break;
                    if (!s.Removed)
                    {
                        n++;
                        yield return s.Value;
                    }
                }
            }
        }
#else
        /// <summary>
        /// A lock free item enumerable.
        /// </summary>
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
#endif
}
