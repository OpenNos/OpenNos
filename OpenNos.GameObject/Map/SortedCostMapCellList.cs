using System.Collections;

namespace OpenNos.GameObject
{
    internal class SortedCostMapCellList
    {
        #region Members

        private MapCellComparer _cellComparer;
        private ArrayList _list;

        #endregion

        #region Instantiation

        public SortedCostMapCellList()
        {
            _list = new ArrayList();
            _cellComparer = new MapCellComparer();
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        #endregion

        #region Methods

        public MapCellAStar CellAt(int i)
        {
            return (MapCellAStar)_list[i];
        }

        public int IndexOf(MapCellAStar n)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                MapCellAStar cellInTheList = (MapCellAStar)_list[i];
                if (cellInTheList.isMatch(n))
                    return i;
            }
            return -1;
        }

        public MapCellAStar pop()
        {
            MapCellAStar r = (MapCellAStar)_list[0];
            _list.RemoveAt(0);
            return r;
        }

        public int push(MapCellAStar n)
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