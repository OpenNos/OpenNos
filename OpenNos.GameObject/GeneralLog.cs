using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class GeneralLog : GeneralLogDTO
    {
        public GeneralLog()
        {
            Mapper.CreateMap<GeneralLogDTO, GeneralLog>();
            Mapper.CreateMap<GeneralLog, GeneralLogDTO>();
        }

    }
}
