using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.Interface;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.EF
{
    // TODO IMPLEMENT
    public class LogChatDAO : MappingBaseDAO<LogChat, LogChatDTO>, ILogChatDAO
    {
        public SaveResult InsertOrUpdate(ref LogChatDTO logchat)
        {
            return SaveResult.Error;
        }
    }
}
