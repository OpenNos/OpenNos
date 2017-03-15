using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using OpenNos.Core;
using System.Xml;
using System.Linq;

namespace OpenNos.GameObject
{
    public class TimeSpace : TimeSpaceDTO
    {
        public TimeSpaceType Type { get; set; }
        public byte LevelMinimum { get; set; }
        public byte LevelMaximum { get; set; }
        public string Label { get; set; }

        public MapInstance FirstMap { get; set; }
        private Dictionary<int, MapInstance> _mapinstancedictionary = new Dictionary<int, MapInstance>();

        public string GenerateRbr()
        {
            return $"rbr 0.0.0 4 15 {LevelMinimum}.{LevelMaximum} 0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 {WinnerScore}.{(WinnerScore > 0 ? Winner : "")} 0 0 {Language.Instance.GetMessageFromKey("TS_TUTORIAL")}\n{Label}";
        }


        public TimeSpace DeepCopy()
        {
            return (TimeSpace)MemberwiseClone();
        }



        public void LoadScript()
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode Maps = doc.SelectSingleNode("Definition").SelectSingleNode("Maps");
                //CreateMaps
                foreach (XmlNode variable in Maps.SelectNodes("Map"))
                {
                    MapInstance newmap = ServerManager.GenerateMapInstance(short.Parse(variable.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance);
                    newmap.MapIndexX = byte.Parse(variable.Attributes["IndexX"].Value);
                    newmap.MapIndexY = byte.Parse(variable.Attributes["IndexY"].Value);
                    _mapinstancedictionary.Add(int.Parse(variable.Attributes["Id"].Value), newmap);
                }
                FirstMap = _mapinstancedictionary.Values.FirstOrDefault();

                foreach (XmlNode variable in Maps.SelectNodes("Map"))
                {
                    MapInstance map = _mapinstancedictionary.First(s => s.Key == int.Parse(variable.Attributes["Id"].Value)).Value;

                    foreach (XmlNode node in variable.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "MapEvents":
                                foreach (XmlNode mapevent in node.ChildNodes)
                                {
                                    switch (mapevent.Name)
                                    {
                                        case "SummonMonsters":
                                            map.EntryEvents.Add(new Tuple<EventActionType, Object>(EventActionType.SPAWN, map.Map.GenerateMonsters(short.Parse(mapevent.Attributes["VNum"].Value), short.Parse(mapevent.Attributes["Amount"].Value), true)));
                                            break;

                                        case "SendPacket":
                                            map.EntryEvents.Add(new Tuple<EventActionType, Object>(EventActionType.SENDPACKET, mapevent.Attributes["Value"].Value));
                                            break;
                                    }
                                }
                                break;

                            case "Portals":
                                foreach (XmlNode portal in node.ChildNodes)
                                {
                                    switch (portal.Name)
                                    {
                                        case "Portal":
                                            MapInstance mapdest = _mapinstancedictionary.First(s => s.Key == int.Parse(portal.Attributes["MapTo"].Value)).Value;
                                            map.Portals.Add(new Portal()
                                            {
                                                SourceX = short.Parse(portal.Attributes["X"].Value),
                                                SourceY = short.Parse(portal.Attributes["Y"].Value),
                                                Type = short.Parse(portal.Attributes["Type"].Value),
                                                DestinationX = short.Parse(portal.Attributes["ToX"].Value),
                                                DestinationY = short.Parse(portal.Attributes["ToY"].Value),
                                                SourceMapInstanceId = map.MapInstanceId,
                                                DestinationMapInstanceId = mapdest.MapInstanceId,
                                            });
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
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