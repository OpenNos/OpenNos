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

using OpenNos.Domain;

namespace OpenNos.Data
{
    public class CharacterDTO
    {
        #region Properties

        public long CharacterId { get; set; }
        public long AccountId { get; set; }
        public short MapId { get; set; }
        public string Name { get; set; }
        public byte Slot { get; set; }
        public byte Gender { get; set; }
        public byte Class { get; set; }
        public byte HairStyle { get; set; }
        public byte HairColor { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int ArenaWinner { get; set; }
        public long Reput { get; set; }
        public short Dignite { get; set; }
        public long Gold { get; set; }
        public int Backpack { get; set; }
        public byte Level { get; set; }
        public long LevelXp { get; set; }
        public byte JobLevel { get; set; }
        public long JobLevelXp { get; set; }
        public int Act4Dead { get; set; }
        public int Act4Kill { get; set; }
        public int Faction { get; set; }
        public int SpPoint { get; set; }
        public int SpAdditionPoint { get; set; }
        public byte State { get; set; }
        public short Compliment { get; set; }
        public bool ExchangeBlocked { get; set; }
        public bool FriendRequestBlocked { get; set; }
        public bool WhisperBlocked { get; set; }
        public bool GroupRequestBlocked { get; set; }
        public bool MouseAimLock { get; set; }
        public bool HeroChatBlocked { get; set; }
        public bool EmoticonsBlocked { get; set; }
        public bool QuickGetUp { get; set; }
        public bool HpBlocked { get; set; }
        public bool BuffBlocked { get; set; }
        public bool MinilandInviteBlocked { get; set; }
        public bool FamilyRequestBlocked { get; set; }
        public int Act4Points { get; set; }
        public int TalentWin { get; set; }
        public int TalentLose { get; set; }
        public int TalentSurrender { get; set; }
        public int MasterPoints { get; set; }
        public int MasterTicket { get; set; }
        public ClassType Classnum { get { return (ClassType)Class; } set { Class = (byte)value; } }
        public CharacterState StateEnum { get { return (CharacterState)State; } set { State = (byte)value; } }

        #endregion
    }
}