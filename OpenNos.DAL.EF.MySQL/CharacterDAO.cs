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

        public DeleteResult Delete(long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Character character = context.character.SingleOrDefault(c => c.CharacterId.Equals(characterId));

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
                Logger.Log.ErrorFormat("Error deleting Character with Id {0} , {1}", characterId, e.Message);
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
                Logger.Log.ErrorFormat("Error inserting or updating character {0} , {1}", character, e.Message);
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
            return Mapper.Map<CharacterDTO>(entity); ;
        }
        private CharacterDTO Update(Character entity, CharacterDTO character, OpenNosContainer context)
        {
            entity = Mapper.Map<Character>(character);
            context.SaveChanges();
            return Mapper.Map<CharacterDTO>(entity);
        }

        #endregion

        #endregion
    }
}
