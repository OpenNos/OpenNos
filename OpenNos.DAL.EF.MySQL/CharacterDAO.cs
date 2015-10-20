using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.EF.MySQL.DB;

namespace OpenNos.DAL.EF.MySQL
{
    public class CharacterDAO : ICharacterDAO
    {
        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach(Character character in context.character.Where(c => c.AccountId.Equals(accountId)).OrderByDescending(c => c.Slot))
                {
                    //TODO implement Object Mapper
                    yield return new CharacterDTO()
                    {
                        CharacterId = character.CharacterId,
                        Class = character.Class,
                        Gender = character.Gender,
                        Gold = character.Gold,
                        HairColor = character.HairColor,
                        HairStyle = character.HairStyle,
                        Hp = character.Hp,
                        JobLevel = character.JobLevel,
                        JobLevelXp = character.JobLevelXp,
                        Level = character.Level,
                        LevelXp = character.LevelXp,
                        Map = character.Map,
                        MapX = character.MapX,
                        MapY = character.MapY,
                        Mp = character.Mp,
                        Name = character.Name,
                        Slot = character.Slot
                    };
                }
            }
        }
    }
}
