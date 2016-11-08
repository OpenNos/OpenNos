using System;

namespace OpenNos.DAL.Interface
{
    public interface IMappingBaseDAO
    {
        #region Methods

        void InitializeMapper();

        IMappingBaseDAO RegisterMapping(Type gameObjectType);

        #endregion
    }
}