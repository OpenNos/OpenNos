using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
    {
    public class BoxItemDTO : SpecialistInstanceDTO, IBoxInstance
    {
        public short HoldingVNum { get; set; }
    }
}
