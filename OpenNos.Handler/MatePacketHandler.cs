using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class MatePacketHandler : IPacketHandler
    {
        public MatePacketHandler(ClientSession session)
        {
            _session = session;
        }

        private ClientSession _session { get; set; }

        /// <summary>
        /// suctl packet
        /// </summary>
        /// <param name="suctlPacket"></param>
        public void Attack(SuctlPacket suctlPacket)
        {
            switch (suctlPacket.TargetType)
            {
                case UserType.Monster:
                    break;

                case UserType.Npc:
                    break;

                case UserType.Player:
                    break;
            }
        }
    }
}
