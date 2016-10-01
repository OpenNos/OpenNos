using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class CharacterDAO : BaseDAO<CharacterDTO>, ICharacterDAO
    {
        #region Methods

        public DeleteResult DeleteByPrimaryKey(long accountId, byte characterSlot)
        {
            CharacterDTO dto = LoadBySlot(accountId, characterSlot);
            Container.Remove(dto);
            return DeleteResult.Deleted;
        }

        public IEnumerable<CharacterDTO> GetTopComplimented()
        {
            return Enumerable.Empty<CharacterDTO>();
        }

        public IEnumerable<CharacterDTO> GetTopPoints()
        {
            return Enumerable.Empty<CharacterDTO>();
        }

        public IEnumerable<CharacterDTO> GetTopReputation()
        {
            return Enumerable.Empty<CharacterDTO>();
        }

        public SaveResult InsertOrUpdate(ref CharacterDTO character)
        {
            CharacterDTO dto = LoadById(character.CharacterId);
            if (dto != null)
            {
                dto = character;
                return SaveResult.Updated;
            }
            else
            {
                Insert(character);
                return SaveResult.Inserted;
            }
        }

        public int IsReputHero(long characterId)
        {
            return 10000;
        }

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            return Container.Where(c => c.AccountId == accountId);
        }

        public CharacterDTO LoadById(long characterId)
        {
            return Container.SingleOrDefault(c => c.CharacterId == characterId);
        }

        public CharacterDTO LoadByName(string name)
        {
            return Container.SingleOrDefault(c => c.Name == name);
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            return Container.SingleOrDefault(c => c.AccountId == accountId && c.Slot == slot);
        }

        #endregion
    }
}