using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface ILogCommandsDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref LogCommandsDTO logCommand);
    }
}
