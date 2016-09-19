using System;

namespace OpenNos.Core
{
    public class PacketIndexAttribute : Attribute
    {
        #region Instantiation

        /// <summary>
        /// Specify the Index of the packet to parse this property to.
        /// </summary>
        /// <param name="index">The zero based index starting from header (exclusive).</param>
        public PacketIndexAttribute(int index)
        {
            Index = index;
        }

        #endregion

        #region Properties

        public int Index { get; set; }

        #endregion
    }
}