using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.EF.MySQL.DB;
using AutoMapper;

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
                    yield return Mapper.Map<CharacterDTO>(character);
                }
            }
        }
       
    }
}
