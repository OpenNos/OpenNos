using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;

namespace OpenNos.DAL.Mock
{
    public class CellonOptionDAO : ICellonOptionDAO
    {
        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(long inventoryitemId)
        {
            throw new NotImplementedException();
        }
    }
}
