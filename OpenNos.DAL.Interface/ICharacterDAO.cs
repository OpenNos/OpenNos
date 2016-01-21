using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.Interface
{
    public interface ICharacterDAO
    {
        CharacterDTO LoadById(long characterId);

        CharacterDTO LoadBySlot(long accountId, byte slot);

        IEnumerable<CharacterDTO> LoadByAccount(long accountId);
     
        SaveResult InsertOrUpdate(ref CharacterDTO character);

        CharacterDTO LoadByName(string name);

        DeleteResult Delete(long accountId, byte characterSlot);

        int IsReputHero(long characterId);

        IEnumerable<CharacterDTO> GetTopComplimented();

        IEnumerable<CharacterDTO> GetTopReputation();

        IEnumerable<CharacterDTO> GetTopPoints();
    }
}
