using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public interface IBoxInstance : ISpecialistInstance
    {
        short HoldingVNum { get; set; }
    }
}
