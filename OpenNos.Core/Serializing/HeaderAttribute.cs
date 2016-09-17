using System;

namespace OpenNos.Core
{
    public class HeaderAttribute : Attribute
    {
        #region Instantiation

        public HeaderAttribute(string identification)
        {
            Identification = identification;
        }

        #endregion

        #region Properties

        public string Identification { get; set; }

        #endregion
    }
}