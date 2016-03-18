using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Core.Threading
{
    public class ThreadedBase<TValue>
    {
        #region Members

        private Action<TValue> _action;
        private SequentialItemProcessor<TValue> _queue;

        #endregion

        //private Task _task;

        #region Instantiation

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

        #endregion

        #region Properties

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

        #endregion
    }

    internal static class CancellationTokenExtensions
    {
        #region Methods

        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }

        #endregion
    }

    internal static class Repeat
    {
        #region Methods

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

        #endregion
    }
}