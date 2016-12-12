/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Core.Threading
{
    public class ThreadedBase<TValue>
    {
        #region Members

        // private Task _task;
        private Action<TValue> _action;

        private SequentialItemProcessor<TValue> _queue;

        #endregion

        #region Instantiation

        public ThreadedBase(long milliseconds, Action<TValue> triggeredMethod)
        {
            _action = triggeredMethod;
            var cancellationTokenSource = new CancellationTokenSource();

            // this will cost a lot of resource _task =
            // Repeat.Interval(TimeSpan.FromMilliseconds(milliseconds), () =>
            // triggeredMethod((TValue)Activator.CreateInstance(typeof(TValue))), cancellationTokenSource.Token);
            Queue.Start();
        }

        #endregion

        #region Properties

        public SequentialItemProcessor<TValue> Queue
        {
            get
            {
                return _queue ?? (_queue = new SequentialItemProcessor<TValue>(_action));
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
            // We don't use Observable.Interval: If we block, the values start bunching up behind
            // each other.
            return Task.Factory.StartNew(
                () =>
                {
                    for (;;)
                    {
                        if (token.WaitCancellationRequested(pollInterval))
                        {
                            break;
                        }
                        action();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion
    }
}