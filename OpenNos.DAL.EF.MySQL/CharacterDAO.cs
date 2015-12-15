using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.DAL.EF.MySQL.DB;
using AutoMapper;
using OpenNos.Core;

namespace OpenNos.DAL.EF.MySQL
{
    public class CharacterDAO : ICharacterDAO
    {
        #region Methods

        #region Public

        public DeleteResult Delete(long accountId, byte characterSlot)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Character character = context.character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(characterSlot));

                    if (character != null)
                    {
                        context.character.Remove(character);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat("DELETE_ERROR", characterSlot, e.Message);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterDTO character)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long characterId = character.CharacterId;
                    Character entity = context.character.SingleOrDefault(c => c.CharacterId.Equals(characterId));

                    if(entity == null) //new entity
                    {
                        character = Insert(character, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        character =  Update(entity, character, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Log.ErrorFormat("INSERT_ERROR", character, e.Message);
                return SaveResult.Error;
            }
        }

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Character character in context.character.Where(c => c.AccountId.Equals(accountId)).OrderByDescending(c => c.Slot))
                {
                    yield return Mapper.Map<CharacterDTO>(character);
                }
            }
        }
       
        public CharacterDTO LoadById(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<CharacterDTO>(context.character.SingleOrDefault(c => c.CharacterId.Equals(characterId)));
            }
        }
        public CharacterDTO LoadByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
               return Mapper.Map<CharacterDTO>(context.character.SingleOrDefault(c => c.Name.Equals(name)));
            }
        }
      
        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<CharacterDTO>(context.character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(slot)));
            }
        }

        #endregion

        #region Private

        private CharacterDTO Insert(CharacterDTO character, OpenNosContainer context)
        {
            Character entity = Mapper.Map<Character>(character);
            context.character.Add(entity);
            context.SaveChanges();
            return Mapper.Map<CharacterDTO>(entity);
        }
        private CharacterDTO Update(Character entity, CharacterDTO character, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.character.SingleOrDefault(c => c.CharacterId == character.CharacterId);
                if (result != null)
                {
                    result.AccountId = character.AccountId;
                    result.CharacterId = character.CharacterId;
                    result.Class = character.Class;
                    result.Dignite = character.Dignite;
                    result.Gender = character.Gender;
                    result.Gold = character.Gold;
                    result.HairColor = character.HairColor;
                    result.HairStyle = character.HairStyle;
                    result.Hp = character.Hp;
                    result.JobLevel = character.JobLevel;
                    result.JobLevelXp = Convert.ToInt32(character.JobLevelXp);
                    result.Level = character.Level;
                    result.LevelXp = Convert.ToInt32( character.LevelXp);
                    result.MapId = character.MapId;
                    result.MapX = character.MapX;
                    result.MapY = character.MapY;
                    result.Mp = character.Mp;
                    result.Name = character.Name;
                    result.Reput = character.Reput;


                    context.SaveChanges();
                }
            }

            return Mapper.Map<CharacterDTO>(entity);
        }

        #endregion

        #endregion
    }
}
