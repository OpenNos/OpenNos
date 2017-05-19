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
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class BCard : BCardDTO
    {
        public override void Initialize()
        {

        }

        public void ApplyBCards(Object session)
        {
            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    if (session.GetType() == typeof(Character))
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            (session as Character).AddBuff(new Buff(SecondData, (session as Character).Level));
                        }
                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {

                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {

                    }
                    else if (session.GetType() == typeof(Mate))
                    {

                    }
                    break;

                case BCardType.CardType.Summons:
                    if (session.GetType() == typeof(Character))
                    {

                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {
                        List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();
                        for (int i = 0; i < FirstData; i++)
                        {
                            short x = (short)(ServerManager.Instance.RandomNumber(-3, 3) + (session as MapMonster).MapX);
                            short y = (short)(ServerManager.Instance.RandomNumber(-3, 3) + (session as MapMonster).MapY);
                            SummonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell() { X = x, Y = y }, -1, true, new List<EventContainer>()));
                        }
                        (session as MapMonster).OnDeathEvents.Add(new EventContainer((session as MapMonster).MapInstance, EventActionType.SPAWNMONSTERS, SummonParameters));
                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {

                    }
                    else if (session.GetType() == typeof(Mate))
                    {

                    }
                    break;
            }
        }
    }
}