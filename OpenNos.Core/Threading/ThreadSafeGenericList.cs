using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    //Definitely not the best approach, but it does what it has to do.
    public class ThreadSafeGenericList<T>
    {
        private List<T> _list;
        private object _sync;

        public ThreadSafeGenericList()
        {
            _list = new List<T>();
            _sync = new object();
        }


        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _list.Count;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_sync)
            {
                return _list.GetEnumerator();
            }
        }

        public void Add(T value)
        {
            lock (_sync)
            {
                _list.Add(value);
            }
        }
        public T FirstOrDefault()
        {
            lock (_sync)
            {
                return _list.FirstOrDefault();
            }
        }

        public void RemoveAll(Predicate<T> match)
        {
            lock (_sync)
            {
                _list.RemoveAll(match);
            }
        }
        public bool Any(Func<T, bool> predicate)
        {
            lock (_sync)
            {
                return _list.Any(predicate);
            }
        }

        public IEnumerable<T> Where(Func<T, bool> p)
        {
            lock (_sync)
            {
                return _list.Where(p);
            }
        }

        public T ElementAt(int v)
        {
            lock (_sync)
            {
                return _list.ElementAt(v);
            }
        }

        public int Sum(Func<T, int> p)
        {
            lock (_sync)
            {
                return _list.Sum(p);
            }
        }

        public void ForEach(Action<T> action)
        {
            lock (_sync)
            {
                _list.ForEach(action);
            }
        }

        public T Single(Func<T, bool> p)
        {
            lock (_sync)
            {
                return _list.Single(p);
            }
        }
    }
}
