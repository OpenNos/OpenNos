using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class CharacterSkillDAO : ICharacterSkillDAO
    {
        #region Methods

        public DeleteResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public DeleteResult Delete(long characterId, short skillVNum)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CharacterSkillDTO> InsertOrUpdate(IEnumerable<CharacterSkillDTO> dtos)
        {
            throw new NotImplementedException();
        }

        public CharacterSkillDTO InsertOrUpdate(CharacterSkillDTO dto)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            throw new NotImplementedException();
        }

        public CharacterSkillDTO LoadById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}