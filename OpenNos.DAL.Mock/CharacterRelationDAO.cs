using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class CharacterRelationDAO : BaseDAO<CharacterRelationDTO>, ICharacterRelationDAO
    {
        #region Methods

        public DeleteResult Delete(long characterId, long relatedCharacterId)
        {
            //CharacterDTO dto = LoadBySlot(accountId, characterSlot);
            //Container.Remove(dto);
            return DeleteResult.Deleted;
        }

        public IList<CharacterRelationDTO> GetBlacklisted(long characterId)
        {
            return new List<CharacterRelationDTO>();
        }

        public IList<CharacterRelationDTO> GetFriends(long characterId)
        {
            return new List<CharacterRelationDTO>();
        }


        public override CharacterRelationDTO Insert(CharacterRelationDTO dto)
        {
            dto.CharacterId = Container.Any() ? Container.Max(c => c.CharacterId) + 1 : 1;
            return base.Insert(dto);
        }

        public SaveResult InsertOrUpdate(ref CharacterRelationDTO character)
        {
            CharacterRelationDTO dto = LoadById(character.CharacterId);
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
        public CharacterRelationDTO LoadById(long characterId)
        {
            return Container.SingleOrDefault(c => c.CharacterId == characterId);
        }
        #endregion
    }
}