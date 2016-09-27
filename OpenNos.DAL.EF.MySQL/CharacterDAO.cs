/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using AutoMapper;
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class CharacterDAO : ICharacterDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public CharacterDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Character, CharacterDTO>();
                cfg.CreateMap<CharacterDTO, Character>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public DeleteResult DeleteByPrimaryKey(long accountId, byte characterSlot)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    // actually a Character wont be deleted, it just will be disabled for future traces
                    byte state = (byte)CharacterState.Active;
                    Character Character = context.Character.FirstOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(characterSlot) && c.State.Equals(state));

                    if (Character != null)
                    {
                        byte obsoleteState = (byte)CharacterState.Inactive;
                        Character.State = obsoleteState;
                        Update(Character, _mapper.Map<CharacterDTO>(Character), context);
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(String.Format(Language.Instance.GetMessageFromKey("DELETE_CHARACTER_ERROR"), characterSlot, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<CharacterDTO> GetTopComplimented()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Character Character in context.Character.Where(c => c.Account.Authority == AuthorityType.User).OrderByDescending(c => c.Compliment).Take(30).ToList())
                {
                    yield return _mapper.Map<CharacterDTO>(Character);
                }
            }
        }

        public IEnumerable<CharacterDTO> GetTopPoints()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Character Character in context.Character.Where(c => c.Account.Authority == AuthorityType.User).OrderByDescending(c => c.Act4Points).Take(30).ToList())
                {
                    yield return _mapper.Map<CharacterDTO>(Character);
                }
            }
        }

        public IEnumerable<CharacterDTO> GetTopReputation()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Character Character in context.Character.Where(c => c.Account.Authority == AuthorityType.User).OrderByDescending(c => c.Reput).Take(43).ToList())
                {
                    yield return _mapper.Map<CharacterDTO>(Character);
                }
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterDTO character)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long CharacterId = character.CharacterId;
                    Character entity = context.Character.FirstOrDefault(c => c.CharacterId.Equals(CharacterId));

                    if (entity == null) 
                    {
                        character = Insert(character, context);
                        return SaveResult.Inserted;
                    }
                    else 
                    { 
                        character = Update(entity, character, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(String.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), character, e.Message), e);
                return SaveResult.Error;
            }
        }

        public int IsReputHero(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                List<Character> heroes = context.Character.Where(c => c.Account.Authority != AuthorityType.Admin).OrderByDescending(c => c.Reput).Take(43).ToList();

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

        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                byte state = (byte)CharacterState.Active;
                foreach (Character Character in context.Character.Where(c => c.AccountId.Equals(accountId) && c.State.Equals(state)).OrderByDescending(c => c.Slot))
                {
                    yield return _mapper.Map<CharacterDTO>(Character);
                }
            }
        }

        public CharacterDTO LoadById(long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<CharacterDTO>(context.Character.FirstOrDefault(c => c.CharacterId.Equals(characterId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public CharacterDTO LoadByName(string name)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    byte state = (byte)CharacterState.Active;
                    return _mapper.Map<CharacterDTO>(context.Character.FirstOrDefault(c => c.Name.Equals(name) && c.State.Equals(state)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    byte state = (byte)CharacterState.Active;
                    return _mapper.Map<CharacterDTO>(context.Character.FirstOrDefault(c => c.AccountId.Equals(accountId) && c.Slot.Equals(slot) && c.State.Equals(state)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private CharacterDTO Insert(CharacterDTO character, OpenNosContext context)
        {
            Character entity = _mapper.Map<Character>(character);
            context.Character.Add(entity);
            context.SaveChanges();
            return _mapper.Map<CharacterDTO>(entity);
        }

        private CharacterDTO Update(Character entity, CharacterDTO character, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(character, entity);
                context.SaveChanges();
            }

            return _mapper.Map<CharacterDTO>(entity);
        }

        #endregion
    }
}