using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface ILogChatDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref LogChatDTO logchat);
    }
}
