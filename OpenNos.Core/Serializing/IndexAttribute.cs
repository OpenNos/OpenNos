using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class IndexAttribute : Attribute
    {
        public int Index { get; set; }

        /// <summary>
        /// Specify the Index of the packet to parse this property to.
        /// </summary>
        /// <param name="index"></param>
        public IndexAttribute(int index)
        {
            Index = index;
        }
    }
}
