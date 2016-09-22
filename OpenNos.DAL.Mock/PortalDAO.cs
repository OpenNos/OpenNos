using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class PortalDAO : IPortalDAO
    {
        #region Methods

        public void Insert(List<PortalDTO> portals)
        {
            throw new NotImplementedException();
        }

        public PortalDTO Insert(PortalDTO portal)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PortalDTO> LoadByMap(short MapId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}