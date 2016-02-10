using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface ICharacterDAO
    {
        #region Methods

        DeleteResult Delete(long accountId, byte characterSlot);

        IEnumerable<CharacterDTO> GetTopComplimented();

        IEnumerable<CharacterDTO> GetTopPoints();

        IEnumerable<CharacterDTO> GetTopReputation();

        SaveResult InsertOrUpdate(ref CharacterDTO character);

        int IsReputHero(long characterId);

        IEnumerable<CharacterDTO> LoadByAccount(long accountId);

        CharacterDTO LoadById(long characterId);

        CharacterDTO LoadByName(string name);

        CharacterDTO LoadBySlot(long accountId, byte slot);

        #endregion
    }
}