using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF
{

    public interface IMappingBaseDAO
    {
        #region Methods

        void InitializeMapper();

        void InitializeMapper(Type baseType);

        IMappingBaseDAO RegisterMapping(Type gameObjectType);

        #endregion
    }

}
