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

namespace OpenNos.Core.Threading
{
    /// <summary>
    /// This class is a timer that performs some tasks periodically.
    /// </summary>
    public class Timer : IDisposable
    {
        #region Members

        /// <summary>
        /// This timer is used to perfom the task at spesified intervals.
        /// </summary>
        private readonly System.Threading.Timer _taskTimer;

        private bool _disposed;

        /// <summary>
        /// Indicates that whether performing the task or _taskTimer is in sleep mode. This field is
        /// used to wait executing tasks when stopping Timer.
        /// </summary>
        private volatile bool _performingTasks;

        /// <summary>
        /// Indicates that whether timer is running or stopped.
        /// </summary>
        private volatile bool _running;

        private object lockObject = new object();

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new Timer.
        /// </summary>
        /// <param name="period">Task period of timer (as milliseconds)</param>
        public Timer(int period) : this(period, false)
        {
        }

        /// <summary>
        /// Creates a new Timer.
        /// </summary>
        /// <param name="period">Task period of timer (as milliseconds)</param>
        /// <param name="runOnStart">
        /// Indicates whether timer raises Elapsed event on Start method of Timer for once
        /// </param>
        public Timer(int period, bool runOnStart)
        {
            Period = period;
            RunOnStart = runOnStart;
            _taskTimer = new System.Threading.Timer(TimerCallBack, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised periodically according to Period of Timer.
        /// </summary>
        public event EventHandler Elapsed;

        #endregion

        #region Properties

        /// <summary>
        /// Task period of timer (as milliseconds).
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Indicates whether timer raises Elapsed event on Start method of Timer for once.
        /// Default: False.
        /// </summary>
        public bool RunOnStart { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            _running = true;
            _taskTimer.Change(RunOnStart ? 0 : Period, Timeout.Infinite);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            lock (lockObject)
            {
                _running = false;
                _taskTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Waits the service to stop.
        /// </summary>
        public void WaitToStop()
        {
            lock (lockObject)
            {
                while (_performingTasks)
                {
                    Monitor.Wait(_taskTimer);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                _taskTimer.Dispose();
            }
        }

        /// <summary>
        /// This method is called by _taskTimer.
        /// </summary>
        /// <param name="state">Not used argument</param>
        private void TimerCallBack(object state)
        {
            lock (lockObject)
            {
                if (!_running || _performingTasks)
                {
                    return;
                }

                _taskTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _performingTasks = true;
            }

            try
            {
                Elapsed?.Invoke(this, new EventArgs());
            }
            catch
            {
            }
            finally
            {
                lock (lockObject)
                {
                    _performingTasks = false;
                    if (_running)
                    {
                        _taskTimer.Change(Period, Timeout.Infinite);
                    }

                    Monitor.Pulse(_taskTimer);
                }
            }
        }

        #endregion
    }
}