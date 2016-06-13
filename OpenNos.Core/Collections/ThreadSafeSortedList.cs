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

using System.Collections.Generic;
using System.Threading;

namespace OpenNos.Core.Collections
{
    /// <summary>
    /// This class is used to store key-value based items in a thread safe manner.
    /// It uses System.Collections.Generic.SortedList publicly.
    /// </summary>
    /// <typeparam name="TK">Key type</typeparam>
    /// <typeparam name="TV">Value type</typeparam>
    public class ThreadSafeSortedList<TK, TV>
    {
        #region Protected Members

        /// <summary>
        /// public collection to store items.
        /// </summary>
        protected readonly SortedList<TK, TV> _items;

        /// <summary>
        /// Used to synchronize access to _items list.
        /// </summary>
        protected readonly ReaderWriterLockSlim _lock;

        #endregion

        #region Public Instantiation

        /// <summary>
        /// Creates a new ThreadSafeSortedList object.
        /// </summary>
        public ThreadSafeSortedList()
        {
            _items = new SortedList<TK, TV>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets count of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region Public Indexers

        /// <summary>
        /// Gets/adds/replaces an item by key.
        /// </summary>
        /// <param name="key">Key to get/set value</param>
        /// <returns>Item associated with this key</returns>
        public TV this[TK key]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.ContainsKey(key) ? _items[key] : default(TV);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _items[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes all items from list.
        /// </summary>
        public void ClearAll()
        {
            _lock.EnterWriteLock();
            try
            {
                _items.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Checks if collection contains spesified key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True; if collection contains given key</returns>
        public bool ContainsKey(TK key)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if collection contains spesified item.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True; if collection contains given item</returns>
        public bool ContainsValue(TV item)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.ContainsValue(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all items in collection.
        /// </summary>
        /// <returns>Item list</returns>
        public List<TV> GetAllItems()
        {
            _lock.EnterReadLock();
            try
            {
                return new List<TV>(_items.Values);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets then removes all items in collection.
        /// </summary>
        /// <returns>Item list</returns>
        public List<TV> GetAndClearAllItems()
        {
            _lock.EnterWriteLock();
            try
            {
                var list = new List<TV>(_items.Values);
                _items.Clear();
                return list;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes an item from collection.
        /// </summary>
        /// <param name="key">Key of item to remove</param>
        public bool Remove(TK key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_items.ContainsKey(key))
                {
                    return false;
                }

                _items.Remove(key);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion
    }
}