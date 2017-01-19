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

using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Family : FamilyDTO
    {
        #region Instantiation
        
        public Family()
        {
            FamilyCharacters = new List<FamilyCharacter>();
        }

        #endregion

        #region Properties

        public List<FamilyCharacter> FamilyCharacters { get; set; }
        public void GenerateLod()
        {
            if (LandOfDeathId == default(Guid))
            {
                LandOfDeathId = ServerManager.GenerateMapInstance(150, MapInstanceType.PersonalInstance);
                
            }
        }
        public Guid LandOfDeathId { get; set; }

        public MapInstance LandOfDeath
        {
            get { return ServerManager.GetMapInstance(LandOfDeathId); }
        }

        public List<FamilyLogDTO> FamilyLogs { get; set; }

        #endregion

        #region Methods

        public override void Initialize()
        {

        }
        public void InsertFamilyLog(FamilyLogType logtype, string CharacterName = "", string CharacterName2 = "", string RainBowFamily = "", string Message = "", byte Level = 0, int Experience = 0, int ItemVNum = 0, byte Upgrade = 0, int RaidType = 0, int right = 0, int righttype = 0,int rightvalue=0)
        {
            string value = string.Empty;
            switch (logtype)
            {
                case FamilyLogType.DailyMessage:
                    value = $"{CharacterName}|{Message}";
                    break;
                case FamilyLogType.FamilyXP:
                    value = $"{CharacterName}|{Experience}";
                    break;
                case FamilyLogType.Level:
                    value = $"{CharacterName}|{Level}";
                    break;
                case FamilyLogType.Raid:
                    value = RaidType.ToString();
                    break;
                case FamilyLogType.Upgrade:
                    value = $"{CharacterName}|{ItemVNum}|{Upgrade}";
                    break;
                case FamilyLogType.UserManage:
                    value = $"{CharacterName}|{CharacterName2}";
                    break;
                case FamilyLogType.FamilyLevel:
                    value = Level.ToString();
                    break;
                case FamilyLogType.AuthorityChange:
                    value = $"{CharacterName}|{right}|{CharacterName2}";
                    break;
                case FamilyLogType.FamilyManage:
                    value = CharacterName;
                    break;
                case FamilyLogType.RainbowBattle:
                    value = RainBowFamily;
                    break;
                case FamilyLogType.RightChange:
                    value = $"{CharacterName}|{right}|{righttype}|{rightvalue}";
                    break;
            }
            FamilyLogDTO log = new FamilyLogDTO
            {
                FamilyId = FamilyId,
                FamilyLogData = value,
                FamilyLogType = logtype,
                Timestamp = DateTime.Now
            };
            DAOFactory.FamilyLogDAO.InsertOrUpdate(ref log);
            ServerManager.Instance.FamilyRefresh(FamilyId);
        }

        #endregion
    }
}