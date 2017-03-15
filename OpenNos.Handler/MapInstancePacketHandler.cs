using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Packets.ServerPackets;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler
{
    class MapInstanceNodePacketHandler : IPacketHandler
    {
        #region Instantiation

        public MapInstanceNodePacketHandler(ClientSession session)
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
            TimeSpace timespace = Session.CurrentMapInstanceNode.Data.TimeSpaces.FirstOrDefault(s => treqPacket.X == s.PositionX && treqPacket.Y == s.PositionY);

            if (timespace != null)
            {
                TimeSpace ts = timespace.DeepCopy();
                ts.LoadContent();
                Session.Character.LastTimeSpace = ts;
                Session.SendPacket(timespace.GenerateRbr());
            }

        }

        /// <summary>
        /// wreq packet
        /// </summary>
        /// <param name="packet"></param>
        public void GetWreq(WreqPacket packet)
        {
            TimeSpace timespace = Session.Character.LastTimeSpace;

            if (timespace != null && timespace.FirstNode != null)
            {
                ServerManager.Instance.ChangeMapInstanceNode(Session.Character.CharacterId, timespace.FirstNode.Data.MapInstanceNodeId, timespace.StartX, timespace.StartY);
                Session.SendPackets(Session.Character.LastTimeSpace.FirstNode.GetMinimap());
            }

        }


        #endregion
    }
}
