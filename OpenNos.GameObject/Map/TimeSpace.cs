using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.DAL.EF;
using System;
using OpenNos.Core;
using NetHierarchy;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Collections;

namespace OpenNos.GameObject
{
    public class TimeSpace : TimeSpaceDTO
    {
        public TimeSpaceType Type { get; set; }
        public byte LevelMinimum { get; set; }
        public byte LevelMaximum { get; set; }
        public short StartX { get; set; }
        public short StartY { get; set; }
        public string Label { get; set; }

        public Node<MapInstance> MapTree { get; set; }

        public string GenerateRbr()
        {
            return $"rbr 0.0.0 4 15 {LevelMinimum}.{LevelMaximum} 0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 {WinnerScore}.{(WinnerScore > 0 ? Winner : "")} 0 0 {Language.Instance.GetMessageFromKey("TS_TUTORIAL")}\n{Label}";
        }


        public TimeSpace DeepCopy()
        {
            return (TimeSpace)MemberwiseClone();
        }


        public void LoadContent()
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode firstMap = doc.SelectSingleNode("Definition").SelectSingleNode("MapTree").SelectSingleNode("Map");
                Node<MapInstance> basemap = new Node<MapInstance>(ServerManager.GenerateMapInstance(short.Parse(firstMap.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance));
                LoadMapInstanceTree(firstMap, ref basemap);
                basemap.Data.MapIndexX = byte.Parse(firstMap.Attributes["IndexX"].Value);
                basemap.Data.MapIndexY = byte.Parse(firstMap.Attributes["IndexY"].Value);
                MapTree = basemap;
            }
        }

        public void LoadMapInstanceTree(XmlNode def, ref Node<MapInstance> Node)
        {
            foreach (XmlNode variable in def.ChildNodes)
            {
                switch (variable.Name)
                {
                    case "SendPacket":
                        Node.Data.EntryEvents.Add(new Tuple<EventActionType, Object>(EventActionType.SENDPACKET, variable.Attributes["Value"].Value));
                        break;
                    case "SummonMonsters":
                        foreach (XmlNode monster in variable.ChildNodes)
                        {
                            if (monster.Name == "Monster")
                            {
                                Node.Data.EntryEvents.Add(new Tuple<EventActionType, Object>(EventActionType.SPAWN, Node.Data.Map.GenerateMonsters(short.Parse(monster.Attributes["VNum"].Value), short.Parse(monster.Attributes["Amount"].Value), true)));
                            }
                        }
                        break;
                    case "Portal":

                        Portal ParentPortal = new Portal()
                        {
                            SourceX = short.Parse(variable.Attributes["ParentPortalX"].Value),
                            SourceY = short.Parse(variable.Attributes["ParentPortalY"].Value),
                            Type = short.Parse(variable.Attributes["ParentPortalType"].Value),
                            DestinationX = short.Parse(variable.Attributes["ChildPortalX"].Value),
                            DestinationY = short.Parse(variable.Attributes["ChildPortalY"].Value),
                            SourceMapInstanceId = Node.Parent.Data.MapInstanceId,
                            DestinationMapInstanceId = Node.Data.MapInstanceId,
                        };
                        Node.Parent.Data.Portals.Add(ParentPortal);
                        Portal childportal = new Portal()
                        {
                            SourceX = short.Parse(variable.Attributes["ChildPortalX"].Value),
                            SourceY = short.Parse(variable.Attributes["ChildPortalY"].Value),
                            Type = short.Parse(variable.Attributes["ChildPortalType"].Value),
                            DestinationX = short.Parse(variable.Attributes["ParentPortalX"].Value),
                            DestinationY = short.Parse(variable.Attributes["ParentPortalY"].Value),
                            SourceMapInstanceId = Node.Data.MapInstanceId,
                            DestinationMapInstanceId = Node.Parent.Data.MapInstanceId,
                        };
                        Node.Data.Portals.Add(childportal);

                        break;

                    case "Map":
                        Node.AddChild(new Node<MapInstance>(ServerManager.GenerateMapInstance(short.Parse(variable.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance)));
                        Node<MapInstance> child = Node.Children.Last();
                        LoadMapInstanceTree(variable, ref child);
                        child.Data.MapIndexX = byte.Parse(variable.Attributes["IndexX"].Value);
                        child.Data.MapIndexY = byte.Parse(variable.Attributes["IndexY"].Value);
                        break;
                }
            }
        }
        public void GenerateMinimap(Node<MapInstance> node, ref List<string> liste)
        {
            liste.Add(node.Data.GenerateRsfn(true));
            foreach (Node<MapInstance> map in node.Children)
            {
                GenerateMinimap(map, ref liste);
            }
        }

        public List<string> GetMinimap()
        {
            List<string> lst = new List<string>();
            lst.Add("rsfm 0 0 4 12");
            GenerateMinimap(MapTree, ref lst);
            return lst;
        }

        public void LoadGlobals()
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode def = doc.SelectSingleNode("Definition");
                LevelMinimum = byte.Parse(def.SelectSingleNode("LevelMinimum").Attributes["Value"].Value);
                LevelMaximum = byte.Parse(def.SelectSingleNode("LevelMaximum").Attributes["Value"].Value);
                Label = def.SelectSingleNode("Label").Attributes["Value"].Value;
                StartX = short.Parse(def.SelectSingleNode("StartX").Attributes["Value"].Value);
                StartY = short.Parse(def.SelectSingleNode("StartY").Attributes["Value"].Value);
            }
        }
    }
}