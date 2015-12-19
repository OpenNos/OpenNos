using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class Npc
    {
        public short NpcId { get; set; }
        public string Name { get; set; }
        public short Vnum { get; set; }
        public short Dialog { get; set; }
        public short MapId { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
        public short Position { get; set; }
        public short Level { get; set; }

        public string GetNpcDialog()
        {
            string dialog = String.Empty;
            if (false)// shop == true)
            {
                //open npcshop
            }
            else
            {
                dialog = String.Format("npc_req 2 {0} {1}", NpcId, Dialog);
            }
            return dialog;
        }
    }
}
