using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Packets.ServerPackets;

namespace OpenNos.Handler
{
    class MapInstancePacketHandler : IPacketHandler
    {
        #region Instantiation

        public MapInstancePacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        /// <summary>
        /// treq packet
        /// </summary>
        /// <param name="treqPacket"></param>
        public void GetTreq(TreqPacket treqPacket)
        {
            TimeSpace timespace = Session.CurrentMapInstance.TimeSpaces.FirstOrDefault(s => treqPacket.X == s.PositionX && treqPacket.Y == s.PositionY);

            if (timespace != null)
            {
                Session.SendPacket(Session.Character.GenerateRbr(timespace));
            }

        }

        #endregion
    }
}
