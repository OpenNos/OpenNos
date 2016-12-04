using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class TeleporterDAO : BaseDAO<TeleporterDTO>, ITeleporterDAO
    {
        #region Methods

        public TeleporterDTO Insert(TeleporterDTO teleporter)
        {
            throw new NotImplementedException();
        }

        public TeleporterDTO LoadById(short TeleporterId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TeleporterDTO> LoadFromNpc(int NpcId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}