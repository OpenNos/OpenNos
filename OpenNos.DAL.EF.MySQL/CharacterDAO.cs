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
using OpenNos.Domain;

namespace OpenNos.DAL.EF.MySQL
{
    public class CharacterDAO : ICharacterDAO
    {
        #region Methods

        #region Public

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                byte state = (byte)CharacterState.Active;
                foreach (Character character in context.character.Where(c => c.AccountId.Equals(accountId) && c.State.Equals(state)).OrderByDescending(c => c.Slot))
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
                byte state = (byte)CharacterState.Active;
                return Mapper.Map<CharacterDTO>(context.character.SingleOrDefault(c => c.Name.Equals(name) && c.State.Equals(state)));
            }
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                byte state = (byte)CharacterState.Active;
                return Mapper.Map<CharacterDTO>(context.character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(slot)
                                                                                        && c.State.Equals(state)));
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

                    if (entity == null) //new entity
                    {
                        character = Insert(character, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        character = Update(entity, character, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat("INSERT_ERROR", character, e.Message);
                return SaveResult.Error;
            }
        }

        public DeleteResult Delete(long accountId, byte characterSlot)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    //actually a character wont be deleted, it just will be disabled for future traces
                    byte state = (byte)CharacterState.Active;
                    Character character = context.character.SingleOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(characterSlot)
                                            && c.State.Equals(state));

                    if (character != null)
                    {
                        byte obsoleteState = (byte)CharacterState.Inactive;
                        character.State = obsoleteState;
                        Update(character, Mapper.Map<CharacterDTO>(character), context);
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

        public int IsReputHero(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                List<Character> heroes = context.character.Where(c => c.account.Authority != (short)AuthorityType.Admin).OrderByDescending(c => c.Reput).Take(43).ToList();

                int i = 0;
                foreach (Character c in heroes)
                {
                    i++;
                    if (c.CharacterId == characterId)
                    {
                        if (i == 1)
                        {
                            return 5;
                        }
                        if (i == 2)
                        {
                            return 4;
                        }
                        if (i == 3)
                        {
                            return 3;
                        }
                        if (i <= 13)
                        {
                            return 2;
                        }
                        if (i <= 43)
                        {
                            return 1;
                        }
                    }
                }
                return 0;
            }
        }

        public IEnumerable<CharacterDTO> GetTopComplimented()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Character character in context.character.Where(c => c.account.Authority != (short)AuthorityType.Admin).OrderByDescending(c => c.Compliment).Take(30).ToList())
                {
                    yield return Mapper.Map<CharacterDTO>(character);
                }
            }
        }

        public IEnumerable<CharacterDTO> GetTopReputation()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Character character in context.character.Where(c=>c.account.Authority != (short)AuthorityType.Admin).OrderByDescending(c =>c.Reput).Take(43).ToList())
                {
                    yield return Mapper.Map<CharacterDTO>(character);
                }
            }
        }

        public IEnumerable<CharacterDTO> GetTopPoints()
        {
            //POINTS NOT IMPLEMENTED RIGHT NOW, STUB
            //using (var context = DataAccessHelper.CreateContext())
            //{
            //    foreach (Character character in context.character.Where(c=>c.account.Authority != (short)AuthorityType.Admin).OrderByDescending(c => c.Compliment).Take(30).ToList())
            //    {
            //        yield return Mapper.Map<CharacterDTO>(character);
            //    }
            //}
            return null;
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
                    result = Mapper.Map<CharacterDTO, Character>(character, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<CharacterDTO>(entity);
        }

        #endregion

        #endregion
    }
}
