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
                GenerateEvent(InstanceEvents);

            }
        }

        private List<EventContainer> GenerateEvent(XmlNode node)
        {
            List<EventContainer> evts = new List<EventContainer>();

            foreach (XmlNode mapevent in node.ChildNodes)
            {
                switch (mapevent.Name)
                {
                    case "InstanceEvents":
                        GenerateEvent(mapevent).ForEach(e => EventHelper.Instance.RunEvent(e));
                        break;
                        
                    case "CreateMap":
                        GenerateEvent(mapevent).ForEach(e => EventHelper.Instance.RunEvent(e));
                        break;
                        
                    case "OnMoveOnMap":
                        _mapinstancedictionary.First(s => s.Key == int.Parse(node.Attributes["Map"].Value)).Value.OnMoveOnMapEvents.AddRange(GenerateEvent(mapevent));
                        break;
                    
                    case "OnCharacterDiscoveringMap":
                        GenerateEvent(mapevent).ForEach(evt => _mapinstancedictionary.First(s => s.Key == int.Parse(node.Attributes["Map"].Value)).Value.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(evt, new List<long>())));
                        break;
                        
                    case "OnDeath":
                        evts.AddRange(GenerateEvent(mapevent));
                        break;

                    case "SummonMonsters":
                        evts.Add(new EventContainer(null, EventActionType.SPAWN, _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["Map"].Value)).Value.Map.GenerateMonsters(short.Parse(mapevent.Attributes["VNum"].Value), short.Parse(mapevent.Attributes["Amount"].Value), true, GenerateEvent(mapevent))));
                        break;

                    case "SendPacket":
                        evts.Add(new EventContainer(null, EventActionType.SENDPACKET, mapevent.Attributes["Value"].Value));
                        break;

                    case "NpcDialog":
                        evts.Add(new EventContainer(null, EventActionType.NPCDIALOG, int.Parse(mapevent.Attributes["Value"].Value)));
                        break;

                    case "SendMessage":
                        evts.Add(new EventContainer(null, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(mapevent.Attributes["Value"].Value, byte.Parse(mapevent.Attributes["Type"].Value))));
                        break;

                    case "GenerateClock":
                        evts.Add(new EventContainer(null, EventActionType.CLOCK, int.Parse(mapevent.Attributes["Value"].Value)));
                        break;

                    case "StartClock":
                        evts.Add(new EventContainer(null, EventActionType.STARTCLOCK, null));
                        break;

                    case "ChangePortalType":
                        evts.Add(new EventContainer(_mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["Map"].Value)).Value, EventActionType.CHANGEPORTALTYPE, mapevent.Attributes["Type"].Value));
                        break;

                    case "SpawnPortal":
                        MapInstance map = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["Map"].Value)).Value;
                        MapInstance destmap = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["MapTo"].Value)).Value;
                        Portal portal = new Portal()
                        {
                            PortalId = byte.Parse(mapevent.Attributes["IdOnMap"].Value),
                            SourceX = short.Parse(mapevent.Attributes["X"].Value),
                            SourceY = short.Parse(mapevent.Attributes["Y"].Value),
                            Type = short.Parse(mapevent.Attributes["Type"].Value),
                            DestinationX = short.Parse(mapevent.Attributes["ToX"].Value),
                            DestinationY = short.Parse(mapevent.Attributes["ToY"].Value),
                            SourceMapInstanceId = map.MapInstanceId,
                            DestinationMapInstanceId = destmap.MapInstanceId,
                        };
                        evts.Add(new EventContainer(map, EventActionType.SPAWNPORTAL, portal));
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