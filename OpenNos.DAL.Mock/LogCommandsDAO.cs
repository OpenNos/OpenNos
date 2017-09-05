using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class LogCommandsDAO : BaseDAO<LogCommandsDTO>, ILogCommandsDAO
    {
        public SaveResult InsertOrUpdate(ref LogCommandsDTO logCommand)
        {
            LogCommandsDTO dto = LoadByLogId(logCommand.CommandId);
            if (dto != null)
            {
                dto = logCommand;
                return SaveResult.Updated;
            }
            Insert(logCommand);
            return SaveResult.Inserted;
        }

        public LogCommandsDTO LoadByLogId(long logId)
        {
            return Container.SingleOrDefault(c => c.CommandId == logId);
        }
    }
}
