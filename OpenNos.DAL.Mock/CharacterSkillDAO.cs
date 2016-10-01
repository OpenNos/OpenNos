using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class CharacterSkillDAO : SynchronizableBaseDAO<CharacterSkillDTO>, ICharacterSkillDAO
    {
        #region Methods

        public DeleteResult Delete(long characterId, short skillVNum)
        {
            CharacterSkillDTO characterSkill = Container.SingleOrDefault(c => c.CharacterId == characterId && c.SkillVNum == skillVNum);
            Container.Remove(characterSkill);
            return DeleteResult.Deleted;
        }

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            return Container.Where(c => c.CharacterId == characterId);
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            return Container.Where(c => c.CharacterId == characterId).Select(c => c.Id);
        }

        #endregion
    }
}