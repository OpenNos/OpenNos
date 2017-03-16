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
    public class TimeSpace : TimeSpaceDTO
    {
        public TimeSpaceType Type { get; set; }
        public byte LevelMinimum { get; set; }
        public byte LevelMaximum { get; set; }
        public string Label { get; set; }
        public MapInstance FirstMap { get; set; }

        MapClock _mapclock = new MapClock();

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
                XmlNode Maps = doc.SelectSingleNode("Definition").SelectSingleNode("Maps");
                //CreateMaps
                int i = 0;
                foreach (XmlNode variable in Maps.SelectNodes("Map"))
                {
                    MapInstance newmap = ServerManager.GenerateMapInstance(short.Parse(variable.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance, _mapclock);
                    newmap.MapIndexX = byte.Parse(variable.Attributes["IndexX"].Value);
                    newmap.MapIndexY = byte.Parse(variable.Attributes["IndexY"].Value);
                    if (i == 0)
                    {
                        FirstMap = newmap;
                    }
                    if (!_mapinstancedictionary.ContainsKey(int.Parse(variable.Attributes["Id"].Value)))
                    {
                        _mapinstancedictionary.Add(int.Parse(variable.Attributes["Id"].Value), newmap);
                    }
                    i++;
                }

                foreach (XmlNode variable in Maps.SelectNodes("Map"))
                {
                    GenerateEvenContainer(variable);
                }
            }
        }

        private void GenerateEvenContainer(XmlNode variable)
        {
            foreach (XmlNode node in variable.ChildNodes)
            {
                MapInstance map;
                List<Tuple<EventActionType, object>> evts = new List<Tuple<EventActionType, object>>();


                switch (node.Name)
                {
                    case "DeathEvents":
                        evts.AddRange(GenerateEvent(node));
                        break;

                    case "MapMoveEvents":
                        map = _mapinstancedictionary.First(s => s.Key == int.Parse(variable.Attributes["Id"].Value)).Value;
                        map.MoveEvents.AddRange(GenerateEvent(node));
                        break;

                    case "MapFirstEntryEvents":
                        map = _mapinstancedictionary.First(s => s.Key == int.Parse(variable.Attributes["Id"].Value)).Value;
                        GenerateEvent(node).ForEach(evt => map.FirstEntryEvents.Add(new Tuple<Tuple<EventActionType, Object>, List<long>>(evt, new List<long>())));
                        break;

                    case "MapCreationEvents":
                        map = _mapinstancedictionary.First(s => s.Key == int.Parse(variable.Attributes["Id"].Value)).Value;
                        GenerateEvent(node).ForEach(s=>map.RunMapEvent(s.Item1,s.Item2));
                        break;
                }
            }
        }

        private List<Tuple<EventActionType, object>> GenerateEvent(XmlNode node)
        {
            List<Tuple<EventActionType, object>> evts = new List<Tuple<EventActionType, object>>();
            foreach (XmlNode mapevent in node.ChildNodes)
            {
                MapInstance map = null;
                MapInstance mapdest = null;
                switch (mapevent.Name)
                {
                    case "SummonMonsters":
                        map = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["Map"].Value)).Value;
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.SPAWN, map.Map.GenerateMonsters(short.Parse(mapevent.Attributes["VNum"].Value), short.Parse(mapevent.Attributes["Amount"].Value), true, GenerateEvent(mapevent))));
                        GenerateEvenContainer(mapevent);
                        break;
                    case "SendPacket":
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.SENDPACKET, mapevent.Attributes["Value"].Value));
                        break;
                    case "NpcDialog":
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.NPCDIALOG, mapevent.Attributes["Value"].Value));
                        break;
                    case "SendMessage":
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.MESSAGE, UserInterfaceHelper.Instance.GenerateMsg(mapevent.Attributes["Value"].Value, byte.Parse(mapevent.Attributes["Type"].Value))));
                        break;
                    case "GenerateClock":
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.CLOCK, int.Parse(mapevent.Attributes["Value"].Value)));
                        break;
                    case "StartClock":
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.STARTCLOCK, null));
                        break;
                    case "SpawnPortal":
                        map = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["Map"].Value)).Value;
                        mapdest = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent.Attributes["MapTo"].Value)).Value;
                        Portal portal = new Portal()
                        {
                            PortalId = short.Parse(mapevent.Attributes["Id"].Value),
                            SourceX = short.Parse(mapevent.Attributes["X"].Value),
                            SourceY = short.Parse(mapevent.Attributes["Y"].Value),
                            Type = short.Parse(mapevent.Attributes["Type"].Value),
                            DestinationX = short.Parse(mapevent.Attributes["ToX"].Value),
                            DestinationY = short.Parse(mapevent.Attributes["ToY"].Value),
                            SourceMapInstanceId = map.MapInstanceId,
                            DestinationMapInstanceId = mapdest.MapInstanceId,
                        };
                        evts.Add(new Tuple<EventActionType, Object>(EventActionType.SPAWNPORTAL, portal));
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