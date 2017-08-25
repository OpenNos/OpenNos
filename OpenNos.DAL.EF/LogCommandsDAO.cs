using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class LogCommandsDAO : MappingBaseDAO<LogCommands, LogCommandsDTO>, ILogCommandsDAO
    {
        public LogCommandsDTO Insert(LogCommandsDTO generallog)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    GeneralLog entity = _mapper.Map<GeneralLog>(generallog);
                    context.GeneralLog.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<LogCommandsDTO>(generallog);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}
