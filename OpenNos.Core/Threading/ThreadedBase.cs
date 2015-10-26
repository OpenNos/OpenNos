using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public abstract class ThreadedBase<TValue>
    {
        private Thread _thread;
        private ConcurrentQueue<TValue> _queue;

        public ThreadedBase()
        {
            _thread = new Thread(Run);
        }

        public void Start()
        {
            _thread.Start();
        }

        public abstract void Run();

        public void Stop()
        {
            _thread.Interrupt();
        }

        public ConcurrentQueue<TValue> Queue
        {
            get
            {
                if(_queue == null)
                {
                    _queue = new ConcurrentQueue<TValue>();
                }

                return _queue;
            }
            set
            {
                _queue = value;
            }
        }
    }
}
