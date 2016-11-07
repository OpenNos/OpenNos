using System;

namespace OpenNos.DAL.Interface
{
    public interface IMappingBaseDAO
    {
        void InitializeMapper();
        IMappingBaseDAO RegisterMapping(Type gameObjectType);
    }
}