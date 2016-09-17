using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class PacketIndexAttribute : Attribute
    {
        public int Index { get; set; }

        public bool HasStringOffset { get; set; }

        /// <summary>
        /// Specify the Index of the packet to parse this property to.
        /// </summary>
        /// <param name="index">The zero based index starting from header (exclusive).</param>
        /// <param name="hasStringOffset">Determines if we ned a - at the end of a string.</param>
        public PacketIndexAttribute(int index, bool hasStringOffset = false)
        {
            Index = index;
            HasStringOffset = hasStringOffset;
        }
    }
}
