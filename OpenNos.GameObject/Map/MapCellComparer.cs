using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    class MapCellComparer : IComparer
    { 
        public int Compare(object x, object y)
        {
            return ((MapCellAStar)x).totalCost - ((MapCellAStar)y).totalCost;
        }
    }
}
