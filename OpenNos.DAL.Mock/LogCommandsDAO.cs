using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class LogCommandsDAO : BaseDAO<LogCommandsDTO>, ILogCommandsDAO
    {
    }
}
