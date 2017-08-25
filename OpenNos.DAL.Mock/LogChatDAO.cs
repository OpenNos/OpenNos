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
        public SaveResult InsertOrUpdate(ref LogChatDTO logchat)
        {
            throw new NotImplementedException();
        }
    }
}
