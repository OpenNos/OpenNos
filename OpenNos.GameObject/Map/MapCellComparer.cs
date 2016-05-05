using System.Collections;

namespace OpenNos.GameObject
{
    internal class MapCellComparer : IComparer
    {
        #region Methods

        public int Compare(object x, object y)
        {
            return ((MapCellAStar)x).TotalCost - ((MapCellAStar)y).TotalCost;
        }

        #endregion
    }
}