using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class Npc : NpcDTO
    {
        public Npc()
        {

            Mapper.CreateMap<NpcDTO, Npc>();
            Mapper.CreateMap<Npc, NpcDTO>();
        }
     
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
