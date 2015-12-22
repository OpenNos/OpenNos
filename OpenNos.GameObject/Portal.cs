using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class Portal : PortalDTO
    {
        public Portal()
        {

            Mapper.CreateMap<PortalDTO, Portal>();
            Mapper.CreateMap<Portal, PortalDTO>();
        }

    }
}
