using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.Interface;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Core;

namespace OpenNos.DAL.EF
{
    public class LogChatDAO : MappingBaseDAO<LogChat, LogChatDTO>, ILogChatDAO
    {
        public DeleteResult DeleteById(long logId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    LogChat log = context.LogChat.First(i => i.LogId.Equals(logId));

                    if (log != null)
                    {
                        context.LogChat.Remove(log);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref LogChatDTO log)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long logId = log.LogId;
                    LogChat entity = context.LogChat.FirstOrDefault(c => c.LogId.Equals(logId));

                    if (entity == null)
                    {
                        log = Insert(log, context);
                        return SaveResult.Inserted;
                    }

                    log.LogId = entity.LogId;
                    log = Update(entity, log, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<LogChatDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (LogChat log in context.LogChat)
                {
                    yield return _mapper.Map<LogChatDTO>(log);
                }
            }
        }

        public LogChatDTO LoadByLogId(long logId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<LogChatDTO>(context.LogChat.FirstOrDefault(i => i.LogId == logId));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        private LogChatDTO Insert(LogChatDTO log, OpenNosContext context)
        {
            try
            {
                LogChat entity = _mapper.Map<LogChat>(log);
                context.LogChat.Add(entity);
                context.SaveChanges();
                return _mapper.Map<LogChatDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private LogChatDTO Update(LogChat entity, LogChatDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(respawn, entity);
                context.SaveChanges();
            }
            return _mapper.Map<LogChatDTO>(entity);
        }
    }
}
