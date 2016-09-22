using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class CharacterDAO : ICharacterDAO
    {
        #region Methods

        public DeleteResult DeleteByPrimaryKey(long accountId, byte characterSlot)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CharacterDTO> GetTopComplimented()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CharacterDTO> GetTopPoints()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CharacterDTO> GetTopReputation()
        {
            throw new NotImplementedException();
        }

        public SaveResult InsertOrUpdate(ref CharacterDTO character)
        {
            throw new NotImplementedException();
        }

        public int IsReputHero(long characterId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            throw new NotImplementedException();
        }

        public CharacterDTO LoadById(long characterId)
        {
            throw new NotImplementedException();
        }

        public CharacterDTO LoadByName(string name)
        {
            throw new NotImplementedException();
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}