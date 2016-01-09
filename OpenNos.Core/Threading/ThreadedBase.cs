using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class ThreadedBase<TValue>
    {
        private SequentialItemProcessor<TValue> _queue;
        private Action<TValue> _action;
        //private Task _task;

        public ThreadedBase(long milliseconds, Action<TValue> triggeredMethod)
        {
            _action = triggeredMethod;
            var cancellationTokenSource = new CancellationTokenSource();
            //this will cost a lot of resource
            //_task = Repeat.Interval(
            //        TimeSpan.FromMilliseconds(milliseconds),
            //        () => triggeredMethod((TValue)Activator.CreateInstance(typeof(TValue))), cancellationTokenSource.Token);
            Queue.Start();
        }

        public SequentialItemProcessor<TValue> Queue
        {
            get
            {
                if (_queue == null)
                {
                    _queue = new SequentialItemProcessor<TValue>(_action);
                }

                return _queue;
            }
            set
            {
                _queue = value;
            }
        }
    }

    internal static class Repeat
    {
        public static Task Interval(
            TimeSpan pollInterval,
            Action action,
            CancellationToken token)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                () =>
                {
                    for (;;)
                    {
                        if (token.WaitCancellationRequested(pollInterval))
                            break;

                        action();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}
