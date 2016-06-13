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

using System.Collections;

namespace OpenNos.GameObject
{
    internal class SortedCostMapCellList
    {
        #region Private Members

        private MapCellComparer _cellComparer;
        private ArrayList _list;

        #endregion

        #region Public Instantiation

        public SortedCostMapCellList()
        {
            _list = new ArrayList();
            _cellComparer = new MapCellComparer();
        }

        #endregion

        #region Public Properties

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        #endregion

        #region Public Methods

        public MapCellAStar CellAt(int i)
        {
            return (MapCellAStar)_list[i];
        }

        public int IndexOf(MapCellAStar n)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                MapCellAStar cellInTheList = (MapCellAStar)_list[i];
                if (cellInTheList.IsMatch(n))
                    return i;
            }
            return -1;
        }

        public MapCellAStar Pop()
        {
            MapCellAStar r = (MapCellAStar)_list[0];
            _list.RemoveAt(0);
            return r;
        }

        public int Push(MapCellAStar n)
        {
            int k = _list.BinarySearch(n, _cellComparer);

            if (k == -1)
                _list.Insert(0, n);
            else if (k < 0)
            {
                k = ~k;
                _list.Insert(k, n);
            }
            else if (k >= 0)
                _list.Insert(k, n);

            return k;
        }

        public void RemoveAt(int i)
        {
            _list.RemoveAt(i);
        }

        #endregion
    }
}