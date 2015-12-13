using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapPacket
    {
        private string characterName;
        private string packet;
        private ReceiverType all;
        #region Instantiation

        public MapPacket(ClientSession session, string content, ReceiverType receiver)
        {
            Session = session;
            Content = content;
            Receiver = receiver;
        }

        public MapPacket(string characterName, string packet, ReceiverType all)
        {
            this.characterName = characterName;
            this.packet = packet;
            this.all = all;
        }

        #endregion

        #region Properties

        public ClientSession Session { get; set; }

        public String Content { get; set; }

        public ReceiverType Receiver { get; set; }

        #endregion
    }
}
