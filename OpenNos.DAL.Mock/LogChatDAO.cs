using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.Interface;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Mock
{
    public class LogChatDAO : BaseDAO<LogChatDTO>, ILogChatDAO
    {
        public SaveResult InsertOrUpdate(ref LogChatDTO logChat)
        {
            LogChatDTO dto = LoadByLogId(logChat.LogId);
            if (dto != null)
            {
                dto = logChat;
                return SaveResult.Updated;
            }
            Insert(logChat);
            return SaveResult.Inserted;
        }

        public LogChatDTO LoadByLogId(long logId)
        {
            return Container.SingleOrDefault(c => c.LogId == logId);
        }
    }
}
