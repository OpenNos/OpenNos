using System;

namespace OpenNos.Core
{
    public class PacketHeaderAttribute : Attribute
    {
        #region Instantiation

        public PacketHeaderAttribute(string identification)
        {
            Identification = identification;
        }

        #endregion

        #region Properties

        public string Identification { get; set; }

        #endregion
    }
}