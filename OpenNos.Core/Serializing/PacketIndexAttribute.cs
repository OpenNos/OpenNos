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
        /// <param name="isReturnPacket">Adds an # to the Header and replaces Spaces with ^ if set to true.</param>
        public PacketIndexAttribute(int index, bool isReturnPacket = false)
        {
            Index = index;
            IsReturnPacket = isReturnPacket;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The zero based index starting from the header (exclusive).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Adds an # to the Header and replaces Spaces with ^
        /// </summary>
        public bool IsReturnPacket { get; set; }

        #endregion
    }
}