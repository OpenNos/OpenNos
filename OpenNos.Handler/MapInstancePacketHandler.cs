using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Packets.ServerPackets;
using OpenNos.GameObject.Helpers;
using CloneExtensions;

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
            ScriptedInstance timespace = Session.CurrentMapInstance.TimeSpaces.FirstOrDefault(s => treqPacket.X == s.PositionX && treqPacket.Y == s.PositionY);

            if (timespace != null)
            {
                Session.Character.LastTimeSpace = timespace;
                Session.SendPacket(timespace.GenerateRbr());
            }

        }

        /// <summary>
        /// wreq packet
        /// </summary>
        /// <param name="packet"></param>
        public void GetWreq(WreqPacket packet)
        {
            Session.Character.LastTimeSpace = Session.Character.LastTimeSpace.GetClone();
            if (Session.Character.LastTimeSpace != null)
            {
                if (Session.Character.LastTimeSpace.FirstMap == null)
                {
                    Session.Character.LastTimeSpace.LoadScript();
                }
                if (Session.Character.LastTimeSpace.FirstMap != null)
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, Session.Character.LastTimeSpace.FirstMap.MapInstanceId);
                    Session.SendPackets(Session.Character.LastTimeSpace.GenerateMinimap());
                }
            }

        }


        #endregion
    }
}
