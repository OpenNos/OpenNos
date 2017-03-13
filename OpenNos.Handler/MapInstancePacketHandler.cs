using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.GameObject;

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

        #endregion
    }
}
