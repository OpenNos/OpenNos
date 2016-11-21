using System;

namespace OpenNos.GameObject.Networking
{
    public class HitRequest : IEquatable<HitRequest>
    {
        #region Instantiation

        public HitRequest()
        {
            HitTimestamp = DateTime.Now;
        }

        #endregion

        #region Properties

        public DateTime HitTimestamp { get; set; }

        #endregion

        #region Methods

        public bool Equals(HitRequest other)
        {
            return other.HitTimestamp == HitTimestamp;
        }

        #endregion
    }
}