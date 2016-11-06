using System;

namespace OpenNos.DAL.Interface
{
    public interface IGameObjectMappingBaseDAO
    {
        void InitializeMapper();
        IGameObjectMappingBaseDAO RegisterMapping(Type gameObjectType);
    }
}