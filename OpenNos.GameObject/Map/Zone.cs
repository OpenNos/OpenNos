using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class Zone
    {

        #region Properties

        public short X { get; set; }

        public short Y { get; set; }

        public short Range { get; set; }

        public bool InZone(short positionX, short positionY)
        {
            return positionX <= X + Range && positionX >= X - Range && positionY <= Y + Range && positionY >= Y - Range ;
        }

        #endregion
    }
}