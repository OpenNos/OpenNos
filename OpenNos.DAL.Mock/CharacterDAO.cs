using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
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

        public IList<CharacterDTO> GetTopComplimented()
        {
            return new List<CharacterDTO>();
        }

        public IList<CharacterDTO> GetTopPoints()
        {
            return new List<CharacterDTO>();
        }

        public IList<CharacterDTO> GetTopReputation()
        {
            return new List<CharacterDTO>();
        }

        public override CharacterDTO Insert(CharacterDTO dto)
        {
            dto.CharacterId = Container.Any() ? Container.Max(c => c.CharacterId) + 1 : 1;
            return base.Insert(dto);
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

        public IList<CharacterDTO> LoadByAccount(long accountId)
        {
            return Container.Where(c => c.AccountId == accountId).Select(e => MapEntity(e)).ToList();
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