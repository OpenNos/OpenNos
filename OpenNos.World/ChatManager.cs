using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.World
{
   public class ChatManager
    {
        private static ChatManager _instance;
        public List<ClientSession> sessions { get; set; }

        private ChatManager() {
            sessions = new List<ClientSession>();
        }

        public static ChatManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ChatManager();

                return _instance;
            }
        }
        
       
    }
}
