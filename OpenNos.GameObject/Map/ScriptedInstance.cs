using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using OpenNos.Core;
using System.Xml;
using System.Linq;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject
{
    public class ScriptedInstance : ScriptedInstanceDTO
    {
        public ScriptedInstanceType Type { get; set; }
        public byte LevelMinimum { get; set; }
        public byte LevelMaximum { get; set; }
        public string Label { get; set; }
        public MapInstance FirstMap { get; set; }

        InstanceBag _instancebag = new InstanceBag();

        Dictionary<int, MapInstance> _mapinstancedictionary = new Dictionary<int, MapInstance>();

        public string GenerateRbr()
        {
            return $"rbr 0.0.0 4 15 {LevelMinimum}.{LevelMaximum} 0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 {WinnerScore}.{(WinnerScore > 0 ? Winner : "")} 0 0 {Language.Instance.GetMessageFromKey("TS_TUTORIAL")}\n{Label}";
        }

        public void Dispose()
        {
            //TODO disposing all maps
        }

        public void LoadScript()
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
                        MapInstance newmap = ServerManager.Instance.GenerateMapInstance(short.Parse(variable.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance, _instancebag);
                        newmap.MapIndexX = byte.Parse(variable.Attributes["IndexX"].Value);
                        newmap.MapIndexY = byte.Parse(variable.Attributes["IndexY"].Value);
                        if (!_mapinstancedictionary.ContainsKey(int.Parse(variable.Attributes["Map"].Value)))
                        {
                            _mapinstancedictionary.Add(int.Parse(variable.Attributes["Map"].Value), newmap);
                        }
                    }
                }

                FirstMap = _mapinstancedictionary.Values.FirstOrDefault();
                GenerateEvent(InstanceEvents, FirstMap);

            }
        }

        private List<EventContainer> GenerateEvent(XmlNode node, MapInstance parentmapinstance)
        {
            List<EventContainer> evts = new List<EventContainer>();


            foreach (XmlNode mapevent in node.ChildNodes)
            {
                int mapid;
                if(mapevent.Attributes["Map"] == null || !int.TryParse(mapevent.Attributes["Map"].Value, out mapid))
                {
                    mapid = -1;
                }
                MapInstance mapinstance = _mapinstancedictionary.FirstOrDefault(s => s.Key == mapid).Value;
                if (mapinstance == null)
                {
                    mapinstance = parentmapinstance;
                }
                switch (mapevent.Name)
                {
                    case "CreateMap":
                    case "InstanceEvents":
                        GenerateEvent(mapevent, mapinstance).ForEach(e => EventHelper.Instance.RunEvent(e));
                        break;

                    case "OnMoveOnMap":
                        mapinstance.OnMoveOnMapEvents.AddRange(GenerateEvent(mapevent, mapinstance));
                        break;

                    case "OnCharacterDiscoveringMap":
                        GenerateEvent(mapevent, mapinstance).ForEach(evt => mapinstance.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(evt, new List<long>())));
                        break;

                    case "OnDeath":
                        evts.AddRange(GenerateEvent(mapevent, mapinstance));
                        break;

                    case "SummonMonsters":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNMONSTERS, mapinstance.Map.GenerateMonsters(short.Parse(mapevent.Attributes["VNum"].Value), short.Parse(mapevent.Attributes["Amount"].Value), true, new List<EventContainer>())));
                        break;

                    case "SummonMonster":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNMONSTERS, mapinstance.Map.GenerateMonsters(short.Parse(mapevent.Attributes["VNum"].Value), 1, true, GenerateEvent(mapevent, mapinstance))));
                        break;

                    case "SpawnButton":
                        MapButton button = new MapButton(
                            mapinstance,
                            int.Parse(mapevent.Attributes["Id"].Value),
                            short.Parse(mapevent.Attributes["PositionX"].Value),
                            short.Parse(mapevent.Attributes["PositionY"].Value),
                             short.Parse(mapevent.Attributes["VNumEnabled"].Value),
                              short.Parse(mapevent.Attributes["VNumDisabled"].Value),
                              new List<EventContainer>(),
                               new List<EventContainer>(),
                                new List<EventContainer>()
                            );
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

                    case "CleanMap":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CLEANMAP, null));
                        break;

                    case "ShowPortals":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SHOWPORTALS, null));
                        break;

                    case "ChangePortalType":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CHANGEPORTALTYPE, new Tuple<int, PortalType>(int.Parse(mapevent.Attributes["IdOnMap"].Value), (PortalType)sbyte.Parse(mapevent.Attributes["Type"].Value))));
                        break;

                    case "SendPacket":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SENDPACKET, mapevent.Attributes["Value"].Value));
                        break;

                    case "NpcDialog":
                        evts.Add(new EventContainer(mapinstance, EventActionType.NPCDIALOG, int.Parse(mapevent.Attributes["Value"].Value)));
                        break;

                    case "SendMessage":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(mapevent.Attributes["Value"].Value, byte.Parse(mapevent.Attributes["Type"].Value))));
                        break;

                    case "GenerateClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CLOCK, int.Parse(mapevent.Attributes["Value"].Value)));
                        break;

                    case "StartClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.STARTCLOCK, null));
                        break;

                    case "SpawnPortal":
                        MapInstance destmap = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["MapTo"].Value)).Value;
                        Portal portal = new Portal()
                        {
                            PortalId = byte.Parse(mapevent.Attributes["IdOnMap"].Value),
                            SourceX = short.Parse(mapevent.Attributes["X"].Value),
                            SourceY = short.Parse(mapevent.Attributes["Y"].Value),
                            Type = short.Parse(mapevent.Attributes["Type"].Value),
                            DestinationX = short.Parse(mapevent.Attributes["ToX"].Value),
                            DestinationY = short.Parse(mapevent.Attributes["ToY"].Value),
                            SourceMapInstanceId = mapinstance.MapInstanceId,
                            DestinationMapInstanceId = destmap.MapInstanceId,
                        };
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNPORTAL, portal));
                        break;
                }

            }
            return evts;
        }


        public List<string> GenerateMinimap()
        {
            List<string> lst = new List<string>();
            lst.Add("rsfm 0 0 4 12");
            _mapinstancedictionary.Values.ToList().ForEach(s => lst.Add(s.GenerateRsfn(true)));
            return lst;
        }

        public void LoadGlobals()
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode def = doc.SelectSingleNode("Definition").SelectSingleNode("Globals");
                LevelMinimum = byte.Parse(def.SelectSingleNode("LevelMinimum").Attributes["Value"].Value);
                LevelMaximum = byte.Parse(def.SelectSingleNode("LevelMaximum").Attributes["Value"].Value);
                Label = def.SelectSingleNode("Label").Attributes["Value"].Value;
            }
        }
    }
}