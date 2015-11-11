using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapPacket
    {
        #region Instantiation

        public MapPacket(ClientSession session, string content, ReceiverType receiver)
        {
            Session = session;
            Content = content;
            Receiver = receiver;
        }

        #endregion

        #region Properties

        public ClientSession Session { get; set; }

        public String Content { get; set; }

        public ReceiverType Receiver { get; set; }

        #endregion
    }
}
