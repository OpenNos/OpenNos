using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IConfigDAO
    {
        ConfigDTO GetOption(short CharacterId,short ConfigId);
        IEnumerable<ConfigDTO> SetOption(short CharacterId,short configId, short value);
    }
}
