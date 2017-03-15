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
using AutoMapper;

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
        public MapInstanceNode FirstNode { get; set; }

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
                MapInstanceNode basemap = ServerManager.GenerateMapInstanceNode(short.Parse(firstMap.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance);
                LoadMapInstanceNodeTree(firstMap, ref basemap);
                basemap.Data.MapIndexX = byte.Parse(firstMap.Attributes["IndexX"].Value);
                basemap.Data.MapIndexY = byte.Parse(firstMap.Attributes["IndexY"].Value);
                FirstNode = basemap;
            }
        }

        public void LoadMapInstanceNodeTree(XmlNode def, ref MapInstanceNode Node)
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
                            SourceMapInstanceNodeId = Node.Parent.Data.MapInstanceNodeId,
                            DestinationMapInstanceNodeId = Node.Data.MapInstanceNodeId,
                        };
                        Node.Parent.Data.Portals.Add(ParentPortal);
                        Portal childportal = new Portal()
                        {
                            SourceX = short.Parse(variable.Attributes["ChildPortalX"].Value),
                            SourceY = short.Parse(variable.Attributes["ChildPortalY"].Value),
                            Type = short.Parse(variable.Attributes["ChildPortalType"].Value),
                            DestinationX = short.Parse(variable.Attributes["ParentPortalX"].Value),
                            DestinationY = short.Parse(variable.Attributes["ParentPortalY"].Value),
                            SourceMapInstanceNodeId = Node.Data.MapInstanceNodeId,
                            DestinationMapInstanceNodeId = Node.Parent.Data.MapInstanceNodeId,
                        };
                        Node.Data.Portals.Add(childportal);

                        break;

                    case "Map":
                        MapInstanceNode child = ServerManager.GenerateMapInstanceNode(short.Parse(variable.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance);
                        child.Parent = Node;
                        LoadMapInstanceNodeTree(variable, ref child);
                        child.Data.MapIndexX = byte.Parse(variable.Attributes["IndexX"].Value);
                        child.Data.MapIndexY = byte.Parse(variable.Attributes["IndexY"].Value);
                        Node.AddChild(child);
                        break;
                }
            }
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