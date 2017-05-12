using System;

namespace OpenNos.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PacketAliasAttribute : Attribute
    {
        #region Instantiation

        public PacketAliasAttribute(string alias)
        {
            Alias = alias;
        }

        #endregion

        #region Properties

        public string Alias { get; set; }

        #endregion
    }
}