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

using OpenNos.Data;
using OpenNos.Domain;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Mate : MateDTO
    {
        #region Instantiation

        public Mate()
        {
        }

        public Mate(Character owner, short VNum, byte level, MateType matetype)
        {
            NpcMonsterVNum = VNum;
            Monster = ServerManager.GetNpc(VNum);
            Hp = Monster.MaxHP;
            Mp = Monster.MaxMP;
            Name = Monster.Name;
            MateType = matetype;
            Level = level;
            MapX = (short)(owner.PositionX + 1);
            MapY = (short)(owner.PositionY + 1);
            Direction = 2;
            CharacterId = owner.CharacterId;
            Owner = owner;
            GetMateTransportId();
            Owner.MapInstance.Broadcast(GenerateIn());
        }

        #endregion

        #region Properties

        public int MateTransportId { get; set; }

        public NpcMonster Monster { get; set; }

        public Character Owner { get; set; }

        #endregion

        #region Methods

        public string GenerateIn()
        {
            return $"in 2 {NpcMonsterVNum} {MateTransportId} {MapX} {MapY} {Direction} 100 100 0 0 3 {CharacterId} 1 0 -1 {Name.Replace(' ', '^')} 0 -1 0 0 0 0 0 0 0 0";
        }

        public string GenerateScPacket()
        {
            switch (MateType)
            {
                case MateType.Partner:
                    return $"{NpcMonsterVNum} {MateTransportId} 32 1000 243248 991.0.0 996.0.0 -1 -1 0 0 1 0 142 174 232 4 70 0 73 158 86 158 69 0 0 0 0 0 2641 2641 1065 1065 0 285816 {Name.Replace(' ', '^')} -1 0 -1 -1 -1 -1";

                case MateType.Pet:
                    return $"{NpcMonsterVNum} {MateTransportId} 1 1000 0 0 0 37 35 27 4 70 0 38 31 17 31 13 0 0 0 3 13 156 156 10 10 0 15 0 {Name.Replace(' ', '^')} 0";
            }
            return string.Empty;
        }

        public void GetMateTransportId()
        {
            int nextId = ServerManager.Instance.MateIds.Any() ? ServerManager.Instance.MateIds.Last() + 1 : 2000000;
            ServerManager.Instance.MateIds.Add(nextId);
            MateTransportId = nextId;
        }

        public override void Initialize()
        {
        }

        #endregion
    }
}