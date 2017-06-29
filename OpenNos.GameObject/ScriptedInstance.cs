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

using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Xml;

namespace OpenNos.GameObject
{
    public class ScriptedInstance : ScriptedInstanceDTO
    {
        #region Members

        private InstanceBag _instancebag = new InstanceBag();

        private Dictionary<int, MapInstance> _mapinstancedictionary = new Dictionary<int, MapInstance>();

        private IDisposable obs;

        #endregion

        #region Properties

        public List<Gift> DrawItems { get; set; }

        public MapInstance FirstMap { get; set; }

        public List<Gift> GiftItems { get; set; }

        public long Gold { get; set; }

        public string Label { get; set; }

        public byte Id { get; set; }

        public byte LevelMaximum { get; set; }

        public byte LevelMinimum { get; set; }

        public byte Lives { get; set; }

        public int MonsterAmount { get; internal set; }

        public int NpcAmount { get; internal set; }

        public int Reputation { get; set; }

        public short StartX { get; set; }

        public short StartY { get; set; }

        public List<Gift> RequieredItems { get; set; }

        public int RoomAmount { get; internal set; }

        public List<Gift> SpecialItems { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            Thread.Sleep(10000);
            _mapinstancedictionary.Values.ToList().ForEach(m => m.Dispose());
        }

        public string GenerateMainInfo()
        {
            return $"minfo 0 1 -1.0/0 -1.0/0 -1/0 -1.0/0 1 {FirstMap.InstanceBag.Lives + 1} 0";
        }

        public List<string> GenerateMinimap()
        {
            List<string> lst = new List<string> { "rsfm 0 0 4 12" };
            _mapinstancedictionary.Values.ToList().ForEach(s => lst.Add(s.GenerateRsfn(true)));
            return lst;
        }

        public string GenerateRbr()
        {
            string drawgift = string.Empty;
            string requireditem = string.Empty;
            string bonusitems = string.Empty;
            string specialitems = string.Empty;

            for (int i = 0; i < 5; i++)
            {
                Gift gift = DrawItems.ElementAtOrDefault(i);
                drawgift += $" {(gift == null ? "-1.0" : $"{gift.VNum}.{gift.Amount}")}";
            }
            for (int i = 0; i < 2; i++)
            {
                Gift gift = SpecialItems.ElementAtOrDefault(i);
                specialitems += $" {(gift == null ? "-1.0" : $"{gift.VNum}.{gift.Amount}")}";
            }

            for (int i = 0; i < 3; i++)
            {
                Gift gift = GiftItems.ElementAtOrDefault(i);
                bonusitems += $"{(i == 0 ? "" : " ")}{(gift == null ? "-1.0" : $"{gift.VNum}.{gift.Amount}")}";
            }
            int WinnerScore = 0;
            string Winner = "";
            return $"rbr 0.0.0 4 15 {LevelMinimum}.{LevelMaximum} {RequieredItems.Sum(s => s.Amount)} {drawgift} {specialitems} {bonusitems} {WinnerScore}.{(WinnerScore > 0 ? Winner : "")} 0 0 {Language.Instance.GetMessageFromKey("TS_TUTORIAL")}\n{Label}";
        }

        public string GenerateWp()
        {
            return $"wp {PositionX} {PositionY} {ScriptedInstanceId} 0 {LevelMinimum} {LevelMaximum}";
        }

        public void LoadGlobals()
        {
            RequieredItems = new List<Gift>();
            DrawItems = new List<Gift>();
            SpecialItems = new List<Gift>();
            GiftItems = new List<Gift>();

            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);

                XmlNode def = doc.SelectSingleNode("Definition").SelectSingleNode("Globals");
                LevelMinimum = byte.Parse(def.SelectSingleNode("LevelMinimum")?.Attributes["Value"].Value);
                LevelMaximum = byte.Parse(def.SelectSingleNode("LevelMaximum")?.Attributes["Value"].Value);
                Label = def.SelectSingleNode("Label")?.Attributes["Value"].Value;
                byte.TryParse(def.SelectSingleNode("Id")?.Attributes["Value"].Value, out byte id);
                Id = id;
                long.TryParse(def.SelectSingleNode("Gold")?.Attributes["Value"].Value, out long gold);
                Gold = gold;
                int.TryParse(def.SelectSingleNode("Reputation")?.Attributes["Value"].Value, out int reputation);
                Reputation = reputation;

                short startx = 0;
                short.TryParse(def.SelectSingleNode("StartX")?.Attributes["Value"].Value, out startx);
                StartX = startx;

                short starty = 0;
                short.TryParse(def.SelectSingleNode("StartY")?.Attributes["Value"].Value, out starty);
                StartY = starty;

                byte lives;
                byte.TryParse(def.SelectSingleNode("Lives")?.Attributes["Value"].Value, out lives);
                Lives = lives;
                if (def.SelectSingleNode("RequieredItems")?.ChildNodes != null)
                {
                    foreach (XmlNode node in def.SelectSingleNode("RequieredItems")?.ChildNodes)
                    {
                        RequieredItems.Add(new Gift(short.Parse(node.Attributes["VNum"].Value), byte.Parse(node.Attributes["Amount"].Value)));
                    }
                }
                if (def.SelectSingleNode("DrawItems")?.ChildNodes != null)
                {
                    foreach (XmlNode node in def.SelectSingleNode("DrawItems")?.ChildNodes)
                    {
                        bool.TryParse(node.Attributes["IsRandomRare"]?.Value, out bool IsRandomRare);
                        short.TryParse(node.Attributes["Design"]?.Value, out short design);
                        DrawItems.Add(new Gift(short.Parse(node.Attributes["VNum"].Value), byte.Parse(node.Attributes["Amount"].Value), design, IsRandomRare));
                    }
                }
                if (def.SelectSingleNode("SpecialItems")?.ChildNodes != null)
                {
                    foreach (XmlNode node in def.SelectSingleNode("SpecialItems")?.ChildNodes)
                    {
                        short.TryParse(node.Attributes["Design"]?.Value, out short design);
                        bool.TryParse(node.Attributes["IsRandomRare"]?.Value, out bool IsRandomRare);
                        SpecialItems.Add(new Gift(short.Parse(node.Attributes["VNum"].Value), byte.Parse(node.Attributes["Amount"].Value), design, IsRandomRare));
                    }
                }
                if (def.SelectSingleNode("GiftItems")?.ChildNodes != null)
                {
                    foreach (XmlNode node in def.SelectSingleNode("GiftItems")?.ChildNodes)
                    {
                        bool.TryParse(node.Attributes["IsRandomRare"]?.Value, out bool IsRandomRare);
                        short.TryParse(node.Attributes["Design"]?.Value, out short design);
                        GiftItems.Add(new Gift(short.Parse(node.Attributes["VNum"].Value), byte.Parse(node.Attributes["Amount"].Value), design, IsRandomRare));
                    }
                }
            }
        }

        public void LoadScript(MapInstanceType mapinstancetype)
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode InstanceEvents = doc.SelectSingleNode("Definition");

                //CreateMaps
                foreach (XmlNode variable in InstanceEvents.SelectSingleNode("InstanceEvents").ChildNodes)
                {
                    if (variable.Name == "CreateMap")
                    {
                        _instancebag.Lives = Lives;
                        MapInstance newmap = ServerManager.Instance.GenerateMapInstance(short.Parse(variable?.Attributes["VNum"].Value), mapinstancetype, _instancebag);
                        byte indexx;
                        byte.TryParse(variable?.Attributes["IndexX"]?.Value, out indexx);
                        newmap.MapIndexX = indexx;

                        byte indexy;
                        byte.TryParse(variable?.Attributes["IndexY"]?.Value, out indexy);
                        newmap.MapIndexY = indexy;

                        if (!_mapinstancedictionary.ContainsKey(int.Parse(variable?.Attributes["Map"].Value)))
                        {
                            _mapinstancedictionary.Add(int.Parse(variable?.Attributes["Map"].Value), newmap);
                        }
                    }
                }

                FirstMap = _mapinstancedictionary.Values.FirstOrDefault();
                Observable.Timer(TimeSpan.FromMinutes(3)).Subscribe(
                   x =>
                   {
                       if (!FirstMap.InstanceBag.Lock)
                       {
                           _mapinstancedictionary.Values.ToList().ForEach(m => EventHelper.Instance.RunEvent(new EventContainer(m, EventActionType.SCRIPTEND, (byte)1)));
                           Dispose();
                       }
                   });
                obs = Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(x =>
                {
                    if (_instancebag.Lives - _instancebag.DeadList.Count() < 0)
                    {
                        _mapinstancedictionary.Values.ToList().ForEach(m => EventHelper.Instance.RunEvent(new EventContainer(m, EventActionType.SCRIPTEND, (byte)3)));
                        Dispose();
                        obs.Dispose();
                    }
                    if (_instancebag.Clock.DeciSecondRemaining <= 0)
                    {
                        _mapinstancedictionary.Values.ToList().ForEach(m => EventHelper.Instance.RunEvent(new EventContainer(m, EventActionType.SCRIPTEND, (byte)1)));
                        Dispose();
                        obs.Dispose();
                    }
                });
                GenerateEvent(InstanceEvents, FirstMap);
            }
        }

        private List<EventContainer> GenerateEvent(XmlNode node, MapInstance parentmapinstance)
        {
            List<EventContainer> evts = new List<EventContainer>();

            foreach (XmlNode mapevent in node.ChildNodes)
            {
                if (mapevent.Name == "#comment")
                {
                    continue;
                }
                int mapid = -1;
                short positionX = -1;
                short positionY = -1;
                short toY = -1;
                short toX = -1;
                int toMap = -1;
                Guid destmapInstanceId = default(Guid);
                if (!int.TryParse(mapevent.Attributes["Map"]?.Value, out mapid))
                {
                    mapid = -1;
                }
                if (!short.TryParse(mapevent.Attributes["PositionX"]?.Value, out positionX) || !short.TryParse(mapevent.Attributes["PositionY"]?.Value, out positionY))
                {
                    positionX = -1;
                    positionY = -1;
                }
                if (int.TryParse(mapevent.Attributes["ToMap"]?.Value, out toMap))
                {
                    MapInstance destmap = _mapinstancedictionary.First(s => s.Key == toMap).Value;
                    if (!short.TryParse(mapevent?.Attributes["ToY"]?.Value, out toY) || !short.TryParse(mapevent?.Attributes["ToX"]?.Value, out toX))
                    {
                        if (destmap != null)
                        {
                            MapCell cell2 = destmap.Map.GetRandomPosition();
                            toY = cell2.Y;
                            toX = cell2.X;
                            destmapInstanceId = destmap.MapInstanceId;
                        }
                        else
                        {
                            toY = -1;
                            toX = -1;
                        }
                    }
                    else
                    {
                        destmapInstanceId = destmap.MapInstanceId;
                    }
                }
                bool.TryParse(mapevent?.Attributes["IsTarget"]?.Value, out bool isTarget);
                bool.TryParse(mapevent?.Attributes["IsBonus"]?.Value, out bool isBonus);
                bool.TryParse(mapevent?.Attributes["IsBoss"]?.Value, out bool isBoss);
                bool.TryParse(mapevent?.Attributes["IsProtected"]?.Value, out bool isProtected);
                bool.TryParse(mapevent?.Attributes["IsMate"]?.Value, out bool isMate);
                if (!bool.TryParse(mapevent?.Attributes["Move"]?.Value, out bool move))
                {
                    move = true;
                }
                if (!bool.TryParse(mapevent?.Attributes["IsHostile"]?.Value, out bool isHostile))
                {
                    isHostile = true;
                }
                MapInstance mapinstance = _mapinstancedictionary.FirstOrDefault(s => s.Key == mapid).Value;
                if (mapinstance == null)
                {
                    mapinstance = parentmapinstance;
                }
                MapCell cell;
                switch (mapevent.Name)
                {
                    //master events
                    case "CreateMap":
                    case "InstanceEvents":
                        GenerateEvent(mapevent, mapinstance).ForEach(e => EventHelper.Instance.RunEvent(e));
                        break;

                    case "End":
                        _mapinstancedictionary.Values.ToList().ForEach(m => evts.Add(new EventContainer(m, EventActionType.SCRIPTEND, byte.Parse(mapevent?.Attributes["Type"].Value))));
                        break;

                    //register events
                    case "OnCharacterDiscoveringMap":
                    case "OnMoveOnMap":
                    case "OnMapClean":
                    case "OnLockerOpen":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REGISTEREVENT, new Tuple<string, List<EventContainer>>(mapevent.Name, GenerateEvent(mapevent, mapinstance))));
                        break;

                    case "OnAreaEntry":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SETAREAENTRY, new ZoneEvent() { X = positionX, Y = positionY, Range = byte.Parse(mapevent?.Attributes["Range"]?.Value), Events = GenerateEvent(mapevent, mapinstance) }));
                        break;

                    case "Wave":
                        byte.TryParse(mapevent?.Attributes["Offset"]?.Value, out byte Offset);
                        evts.Add(new EventContainer(mapinstance, EventActionType.REGISTERWAVE, new EventWave(byte.Parse(mapevent?.Attributes["Delay"]?.Value), GenerateEvent(mapevent, mapinstance), Offset)));
                        break;

                    case "SetMonsterLockers":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SETMONSTERLOCKERS, byte.Parse(mapevent?.Attributes["Value"]?.Value)));
                        break;

                    case "SetButtonLockers":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SETBUTTONLOCKERS, byte.Parse(mapevent?.Attributes["Value"]?.Value)));
                        break;
                    case "ControlMonsterInRange":
                        short.TryParse(mapevent?.Attributes["VNum"]?.Value, out short vnum);
                        evts.Add(new EventContainer(mapinstance, EventActionType.CONTROLEMONSTERINRANGE, new Tuple<short, byte, List<EventContainer>>(vnum, byte.Parse(mapevent?.Attributes["Range"]?.Value), GenerateEvent(mapevent, mapinstance))));
                        //Tuple<short, byte, List<EventContainer>>
                        break;
                    //child events
                    case "OnDeath":
                        evts.AddRange(GenerateEvent(mapevent, mapinstance));
                        break;

                    case "OnTarget":
                        evts.Add(new EventContainer(mapinstance, EventActionType.ONTARGET,  GenerateEvent(mapevent, mapinstance)));
                        break;

                    case "Effect":
                        evts.Add(new EventContainer(mapinstance, EventActionType.EFFECT, short.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "SummonMonsters":
                        MonsterAmount += short.Parse(mapevent?.Attributes["Amount"].Value);
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNMONSTERS, mapinstance.Map.GenerateMonsters(short.Parse(mapevent?.Attributes["VNum"].Value), short.Parse(mapevent?.Attributes["Amount"].Value), move, new List<EventContainer>(), isBonus, isHostile, isBoss)));
                        break;

                    case "SummonMonster":
                        if (positionX == -1 || positionY == -1)
                        {
                            cell = mapinstance?.Map?.GetRandomPosition();
                            if (cell != null)
                            {
                                positionX = cell.X;
                                positionY = cell.Y;
                            }
                        }
                        MonsterAmount++;
                        List<EventContainer> notice = new List<EventContainer>();
                        List<EventContainer> death = new List<EventContainer>();
                        byte noticerange = 0;
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnDeath":
                                    death.AddRange(GenerateEvent(var, mapinstance));
                                    break;

                                case "OnNoticing":
                                    byte.TryParse(var?.Attributes["Range"]?.Value, out noticerange);
                                    notice.AddRange(GenerateEvent(var, mapinstance));
                                    break;

                            }
                        }
                        List<MonsterToSummon> lst = new List<MonsterToSummon>
                        {
                            new MonsterToSummon(short.Parse(mapevent?.Attributes["VNum"].Value), new MapCell() { X = positionX, Y = positionY }, -1, move, isTarget, isBonus, isHostile, isBoss)
                            {
                                DeathEvents = death,
                                NoticingEvents = notice,
                                NoticeRange = noticerange
                            }
                        };
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNMONSTERS, lst.AsEnumerable()));
                        break;

                    case "SummonNps":
                        NpcAmount += short.Parse(mapevent?.Attributes["Amount"].Value); ;
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNNPCS,
                            mapinstance.Map.GenerateNpcs(short.Parse(mapevent?.Attributes["VNum"].Value),
                            short.Parse(mapevent?.Attributes["Amount"].Value), new List<EventContainer>(), isMate, isProtected)));
                        break;

                    case "RefreshRaidGoals":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REFRESHRAIDGOAL, null));
                        break;

                    case "Move":
                        List<EventContainer> moveevents = new List<EventContainer>();
                        moveevents.AddRange(GenerateEvent(mapevent, mapinstance));
                        evts.Add(new EventContainer(mapinstance, EventActionType.MOVE, new ZoneEvent() { X = positionX, Y = positionY, Events = moveevents }));
                        break;

                    case "SummonNpc":
                        if (positionX == -1 || positionY == -1)
                        {
                            cell = mapinstance?.Map?.GetRandomPosition();
                            if (cell != null)
                            {
                                positionX = cell.X;
                                positionY = cell.Y;
                            }
                        }
                        NpcAmount++;
                        List<NpcToSummon> lstn = new List<NpcToSummon>
                        {
                            new NpcToSummon(short.Parse(mapevent?.Attributes["VNum"].Value), new MapCell() { X = positionX, Y = positionY }, -1, GenerateEvent(mapevent, mapinstance), isMate, isProtected)
                        };
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNNPCS, lstn.AsEnumerable()));
                        break;

                    case "SpawnButton":
                        if (positionX == -1 || positionY == -1)
                        {
                            cell = mapinstance?.Map?.GetRandomPosition();
                            if (cell != null)
                            {
                                positionX = cell.X;
                                positionY = cell.Y;
                            }
                        }
                        MapButton button = new MapButton(
                            int.Parse(mapevent?.Attributes["Id"].Value), positionX, positionY,
                            short.Parse(mapevent?.Attributes["VNumEnabled"].Value),
                            short.Parse(mapevent?.Attributes["VNumDisabled"].Value), new List<EventContainer>(), new List<EventContainer>(), new List<EventContainer>());
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnFirstEnable":
                                    button.FirstEnableEvents.AddRange(GenerateEvent(var, mapinstance));
                                    break;

                                case "OnEnable":
                                    button.EnableEvents.AddRange(GenerateEvent(var, mapinstance));
                                    break;

                                case "OnDisable":
                                    button.DisableEvents.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNBUTTON, button));
                        break;

                    case "StopClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.STOPCLOCK, null));
                        break;

                    case "StopMapClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.STOPMAPCLOCK, null));
                        break;

                    case "RefreshMapItems":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REFRESHMAPITEMS, null));
                        break;

                    case "RemoveMonsterLocker":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REMOVEMONSTERLOCKER, null));
                        break;

                    case "ThrowItem":
                        short.TryParse(mapevent?.Attributes["VNum"]?.Value, out short vnum2);
                        byte.TryParse(mapevent?.Attributes["PackAmount"]?.Value, out byte packAmount);
                        int.TryParse(mapevent?.Attributes["MinAmount"]?.Value, out int minAmount);
                        int.TryParse(mapevent?.Attributes["MaxAmount"]?.Value, out int maxAmount);
                        evts.Add(new EventContainer(mapinstance, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int>(-1, vnum2, packAmount == 0 ? (byte)1 : packAmount, minAmount == 0 ? 1 : minAmount, maxAmount == 0 ? 1 : maxAmount)));
                        break;

                    case "RemoveButtonLocker":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REMOVEBUTTONLOCKER, null));
                        break;

                    case "ChangePortalType":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CHANGEPORTALTYPE,
                            new Tuple<int, PortalType>(int.Parse(mapevent?.Attributes["IdOnMap"].Value), (PortalType)sbyte.Parse(mapevent?.Attributes["Type"].Value))));
                        break;

                    case "SendPacket":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SENDPACKET, mapevent?.Attributes["Value"].Value));
                        break;

                    case "NpcDialog":
                        evts.Add(new EventContainer(mapinstance, EventActionType.NPCDIALOG, int.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "SendMessage":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(mapevent?.Attributes["Value"].Value, byte.Parse(mapevent?.Attributes["Type"].Value))));
                        break;

                    case "GenerateClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CLOCK, int.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "GenerateMapClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.MAPCLOCK, int.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "Teleport":
                        evts.Add(new EventContainer(mapinstance, EventActionType.TELEPORT, new Tuple<short, short, short, short>(short.Parse(mapevent?.Attributes["PositionX"].Value), short.Parse(mapevent?.Attributes["PositionY"].Value), short.Parse(mapevent?.Attributes["DestinationX"].Value), short.Parse(mapevent?.Attributes["DestinationY"].Value))));
                        break;

                    case "StartClock":
                        Tuple<List<EventContainer>, List<EventContainer>> eve = new Tuple<List<EventContainer>, List<EventContainer>>(new List<EventContainer>(), new List<EventContainer>());
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnTimeout":
                                    eve.Item1.AddRange(GenerateEvent(var, mapinstance));
                                    break;

                                case "OnStop":
                                    eve.Item2.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.STARTCLOCK, eve));
                        break;

                    case "StartMapClock":
                        eve = new Tuple<List<EventContainer>, List<EventContainer>>(new List<EventContainer>(), new List<EventContainer>());
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnTimeout":
                                    eve.Item1.AddRange(GenerateEvent(var, mapinstance));
                                    break;

                                case "OnStop":
                                    eve.Item2.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.STARTMAPCLOCK, eve));
                        break;

                    case "SpawnPortal":
                        Portal portal = new Portal()
                        {
                            PortalId = byte.Parse(mapevent?.Attributes["IdOnMap"].Value),
                            SourceX = positionX,
                            SourceY = positionY,
                            Type = short.Parse(mapevent?.Attributes["Type"].Value),
                            DestinationX = toX,
                            DestinationY = toY,
                            DestinationMapId = (short)(destmapInstanceId == default(Guid) ? -1 : 0),
                            SourceMapInstanceId = mapinstance.MapInstanceId,
                            DestinationMapInstanceId = destmapInstanceId,
                        };
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            if (var.Name == "OnTraversal")
                            {
                                portal.OnTraversalEvents.AddRange(GenerateEvent(var, mapinstance));
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNPORTAL, portal));
                        break;
                }
            }
            return evts;
        }

        #endregion
    }
}