using OpenNos.Data;

namespace OpenNos.DAL.Interface
{
    public interface IConfigDAO
    {
        #region Methods

        ConfigDTO GetOption(short CharacterId, short configId);

        ConfigDTO SetOption(short CharacterId, short configId, short Value);

        #endregion
    }
}