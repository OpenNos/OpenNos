using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class TeleporterDAO : ITeleporterDAO
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