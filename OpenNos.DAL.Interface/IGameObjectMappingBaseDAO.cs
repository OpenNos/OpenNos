using System;

namespace OpenNos.DAL.Interface
{
    public interface IGameObjectMappingBaseDAO
    {
        void InitializeMapper();
        void RegisterMapping(Type gameObjectType);
    }
}