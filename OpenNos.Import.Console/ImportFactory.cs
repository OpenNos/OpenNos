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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenNos.Import.Console
{
    public class ImportFactory
    {
        #region Members

        private readonly string _folder;
        private List<string[]> packetList = new List<string[]>();
        private IEnumerable<MapDTO> Maps = null;
        #endregion

        #region Instantiation

        public ImportFactory(string folder)
        {
            _folder = folder;
        }

        #endregion

        #region Methods
        public void loadMaps()
        {
            Maps = DAOFactory.MapDAO.LoadAll();
        }
        public void ImportMaps()
        {
            string fileMapIdDat = $"{_folder}\\MapIDData.dat";
            string fileMapIdLang = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt";
            string folderMap = $"{_folder}\\map";

            Dictionary<int, string> dictionaryId = new Dictionary<int, string>();
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            Dictionary<int, int> dictionaryMusic = new Dictionary<int, int>();

            string line;
            int i = 0;
            using (StreamReader mapIdStream = new StreamReader(fileMapIdDat, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split(' ');
                    if (linesave.Length > 1)
                    {
                        int mapid;
                        if (!int.TryParse(linesave[0], out mapid)) continue;

                        if (!dictionaryId.ContainsKey(mapid))
                            dictionaryId.Add(mapid, linesave[4]);
                    }
                }
                mapIdStream.Close();
            }

            using (StreamReader mapIdLangStream = new StreamReader(fileMapIdLang, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }
                mapIdLangStream.Close();
            }

            foreach (string[] linesave in packetList.Where(o => o[0].Equals("at")))
            {
                if (linesave.Length > 7 && linesave[0] == "at")
                {
                    if (!dictionaryMusic.ContainsKey(int.Parse(linesave[2])))
                        dictionaryMusic.Add(int.Parse(linesave[2]), int.Parse(linesave[7]));
                }
            }

            foreach (FileInfo file in new DirectoryInfo(folderMap).GetFiles())
            {
                string name = "";
                int music = 0;
                if (dictionaryId.ContainsKey(int.Parse(file.Name)) && dictionaryIdLang.ContainsKey(dictionaryId[int.Parse(file.Name)]))
                    name = dictionaryIdLang[dictionaryId[int.Parse(file.Name)]];

                if (dictionaryMusic.ContainsKey(int.Parse(file.Name)))
                    music = dictionaryMusic[int.Parse(file.Name)];

                MapDTO map = new MapDTO
                {
                    Name = name,
                    Music = music,
                    MapId = short.Parse(file.Name),
                    Data = File.ReadAllBytes(file.FullName)
                };
                if (DAOFactory.MapDAO.LoadById(map.MapId) != null) continue; // Map already exists in list

                DAOFactory.MapDAO.Insert(map);
                i++;
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPS_PARSED"), i));
        }

        internal void ImportItems()
        {
            string fileId = $"{_folder}\\Item.dat";
            string fileLang = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_Item.txt";
            Dictionary<string, string> dictionaryName = new Dictionary<string, string>();
            string line = string.Empty;
            using (StreamReader mapIdLangStream = new StreamReader(fileLang, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryName.ContainsKey(linesave[0]))
                    {
                        dictionaryName.Add(linesave[0], linesave[1]);
                    }
                }
                mapIdLangStream.Close();
            }

            using (StreamReader npcIdStream = new StreamReader(fileId, Encoding.GetEncoding(1252)))
            {
                ItemDTO item = new ItemDTO();
                bool itemAreaBegin = false;
                string name = "";
                int i = 0;
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');

                    if (linesave.Length > 3 && linesave[1] == "VNUM")
                    {
                        itemAreaBegin = true;
                        item.VNum = short.Parse(linesave[2]);
                        item.Price = long.Parse(linesave[3])/20;
                    }
                    else if (linesave.Length > 1 && linesave[1] == "END")
                    {
                        if (!itemAreaBegin) continue;

                        if (DAOFactory.ItemDAO.LoadById(item.VNum) == null)
                        {
                            DAOFactory.ItemDAO.Insert(item);
                            i++;
                        }
                        item = new ItemDTO();
                        itemAreaBegin = false;
                    }
                    else if (linesave.Length > 2 && linesave[1] == "NAME")
                    {
                        if (dictionaryName.TryGetValue(linesave[2].ToString(), out name))
                        {
                            item.Name = name;
                        }
                        else
                            item.Name = string.Empty;
                    }
                    else if (linesave.Length > 7 && linesave[1] == "INDEX")
                    {
                        item.Type = Convert.ToByte(linesave[2]) != 4 ? Convert.ToByte(linesave[2]) : (byte)0;
                        item.ItemType = linesave[3] != "-1" ? Convert.ToByte($"{item.Type}{linesave[3]}") : (byte)0;
                        item.ItemSubType = Convert.ToByte(linesave[4]);
                        item.EquipmentSlot = Convert.ToByte(linesave[5] != "-1" ? linesave[5] : "0");
                        //linesave[6] design id?
                        item.Morph = Convert.ToInt16(linesave[7]);
                    }
                    else if (linesave.Length > 3 && linesave[1] == "TYPE")
                    {
                        //linesave[2] 0-range 2-range 3-magic but useless
                        if (item.EquipmentSlot == (byte)EquipmentType.Fairy)
                            item.Class = 15;
                        else
                            item.Class = Convert.ToByte(linesave[3]);
                    }
                    else if (linesave.Length > 3 && linesave[1] == "FLAG")
                    {
                        item.IsBlocked = linesave[5] == "1" ? true : false;
                        item.IsDroppable = linesave[6] == "0" ? true : false;
                        item.IsTradable = linesave[7] == "0" ? true : false;
                        item.IsSoldable = linesave[8] == "0" ? true : false;
                        item.IsMinilandObject = linesave[9] == "1" ? true : false;
                        item.IsWarehouse = linesave[10] == "1" ? true : false;
                        item.IsColored = linesave[16] == "1" ? true : false;
                        item.Sex = linesave[18] == "1" ? (byte)1 : linesave[17] == "1" ? (byte)2 : (byte)0;

                        /*
                        ??item.IsVehicle = linesave[11] == "1" ? true : false;??
                        ??item.BoxedVehicle = linesave[12] == "1" ? true : false;??
                        linesave[2]  not used
                        linesave[3]  not used
                        linesave[4]  idk
                        linesave[11] idk
                        linesave[12] idk
                        linesave[13] idk
                        linesave[14] idk
                        linesave[15] idk
                        linesave[19] idk
                        linesave[20] idk
                        linesave[21] idk
                        */
                    }
                    else if (linesave.Length > 1 && linesave[1] == "DATA")
                    {

                        switch (item.ItemType)
                        {
                            case (byte)ItemType.Weapon:
                                item.LevelMinimum = Convert.ToByte(linesave[2]);
                                item.DamageMinimum = Convert.ToInt16(linesave[3]);
                                item.DamageMaximum = Convert.ToInt16(linesave[4]);
                                item.HitRate = Convert.ToInt16(linesave[5]);
                                item.CriticalLuckRate = Convert.ToInt16(linesave[6]);
                                item.CriticalRate = Convert.ToInt16(linesave[7]);
                                item.BasicUpgrade = Convert.ToByte(linesave[10]);
                                break;
                            case (byte)ItemType.Armor:
                                item.LevelMinimum = Convert.ToByte(linesave[2]);
                                item.CloseDefence = Convert.ToInt16(linesave[3]);
                                item.DistanceDefence = Convert.ToInt16(linesave[4]);
                                item.MagicDefence = Convert.ToInt16(linesave[5]);
                                item.DefenceDodge = Convert.ToInt16(linesave[6]);
                                item.DistanceDefenceDodge = Convert.ToInt16(linesave[6]);
                                item.BasicUpgrade = Convert.ToByte(linesave[10]);
                                break;
                            case (byte)ItemType.Box:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[3]);
                                break;
                            case (byte)ItemType.Fashion:
                                item.LevelMinimum = Convert.ToByte(linesave[2]);
                                item.CloseDefence = Convert.ToInt16(linesave[3]);
                                item.DistanceDefence = Convert.ToInt16(linesave[4]);
                                item.MagicDefence = Convert.ToInt16(linesave[5]);
                                item.DefenceDodge = Convert.ToInt16(linesave[6]);
                                item.ItemValidTime = Convert.ToInt32(linesave[13]) * 3600;
                                break;
                            case (byte)ItemType.Food:
                                item.Hp = Convert.ToInt16(linesave[2]);
                                item.Mp = Convert.ToInt16(linesave[4]);
                                break;
                            case (byte)ItemType.Jewelery:
                                if (item.EquipmentSlot.Equals((byte)EquipmentType.Amulet))
                                {
                                    item.Effect = Convert.ToInt16(linesave[2]);
                                    item.EffectValue = Convert.ToInt32(linesave[3]);
                                }
                                else if (item.EquipmentSlot.Equals((byte)EquipmentType.Fairy))
                                {
                                    item.Element = Convert.ToByte(linesave[2]);
                                    item.ElementRate = Convert.ToInt16(linesave[3]);
                                }
                                else
                                {
                                    item.LevelMinimum = Convert.ToByte(linesave[2]);
                                    item.MaxCellonLvl = Convert.ToByte(linesave[3]);
                                    item.MaxCellon = Convert.ToByte(linesave[4]);
                                }
                                break;
                            case (byte)ItemType.Special:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                break;
                            case (byte)ItemType.Magical:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                break;
                            case (byte)ItemType.Specialist:
                                //item.isSpecialist = Convert.ToByte(linesave[2]);

                                item.Speed = Convert.ToByte(linesave[5]);
                                item.SpType = Convert.ToByte(linesave[13]);
                                //item.Morph = Convert.ToInt16(linesave[14]) + 1; // idk whats that, its useless
                                item.FireResistance = Convert.ToByte(linesave[15]);
                                item.WaterResistance = Convert.ToByte(linesave[16]);
                                item.LightResistance = Convert.ToByte(linesave[17]);
                                item.DarkResistance = Convert.ToByte(linesave[18]);
                                //item.PartnerClass = Convert.ToInt16(linesave[19]);
                                item.LevelJobMinimum = Convert.ToByte(linesave[20]);
                                item.ReputationMinimum = Convert.ToByte(linesave[21]);
                                Dictionary<int, int> Elementdic = new Dictionary<int, int>();
                                Elementdic.Add(0, 0);
                                if (item.FireResistance != 0)
                                    Elementdic.Add(1, item.FireResistance);
                                if (item.WaterResistance != 0)
                                    Elementdic.Add(2, item.WaterResistance);
                                if (item.LightResistance != 0)
                                    Elementdic.Add(3, item.LightResistance);
                                if (item.DarkResistance != 0)
                                    Elementdic.Add(4, item.DarkResistance);
                                item.Element = (byte)Elementdic.OrderByDescending(s => s.Value).First().Key;
                                if (Elementdic.Count > 1 && Elementdic.OrderByDescending(s => s.Value).First().Value == Elementdic.OrderByDescending(s => s.Value).ElementAt(1).Value)
                                {
                                    item.SecondaryElement = (byte)Elementdic.OrderByDescending(s => s.Value).ElementAt(1).Key;
                                }
                                if (item.VNum == 903) //need to hardcode...
                                    item.Element = 2;
                                else if (item.VNum == 901)//need to hardcode...
                                    item.Element = 1;
                                break;
                            case (byte)ItemType.Shell:
                                //item.ShellMinimumLevel = Convert.ToInt16(linesave[3]);//wtf\/\/ this two things are wrong in many ways
                                //item.ShellMaximumLevel = Convert.ToInt16(linesave[4]);//wtf/\/\ this two things are wrong in many ways
                                //item.ShellType = Convert.ToByte(linesave[5]); //3 shells of each type
                                break;
                            case (byte)ItemType.Main:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                break;
                            case (byte)ItemType.Upgrade:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                break;
                            case (byte)ItemType.Production:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                break;
                            case (byte)ItemType.Map:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                break;
                            case (byte)ItemType.Potion:
                                item.Hp = Convert.ToInt16(linesave[2]);
                                item.Mp = Convert.ToInt16(linesave[4]);
                                break;
                            case (byte)ItemType.Snack:
                                item.Hp = Convert.ToInt16(linesave[2]);
                                item.Mp = Convert.ToInt16(linesave[4]);
                                break;
                            case (byte)ItemType.Teacher:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.EffectValue = Convert.ToInt32(linesave[4]);
                                //item.PetLoyality = Convert.ToInt16(linesave[4]);
                                //item.PetFood = Convert.ToInt16(linesave[7]);
                                break;
                        
                            case (byte)ItemType.Part:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Sell:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Quest2:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Quest1:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Ammo:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Event:
                                //nothing to parse
                                break;
                        }

                    }
                    else if (linesave.Length > 1 && linesave[1] == "BUFF")
                    {
                        //need to see how to use them :D (we know how to get the buff from bcard ect)
                    }
                }
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ITEMS_PARSED"), i));
                npcIdStream.Close();
            }


        }

        public void ImportNpcs()
        {
            string fileNpcId = $"{_folder}\\monster.dat";
            string fileNpcLang = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_monster.txt";

            // Store like this: (vnum, (name, level))

            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            NpcDTO npc = new NpcDTO();
            string line;
            bool itemAreaBegin = false;
            List<NpcDTO> npclist = new List<NpcDTO>();
            using (StreamReader npcIdLangStream = new StreamReader(fileNpcLang, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                }
                npcIdLangStream.Close();
            }


            using (StreamReader npcIdStream = new StreamReader(fileNpcId, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdStream.ReadLine()) != null)
                {

                    string[] linesave = line.Split('\t');

                    if (linesave.Length > 2 && linesave[1] == "VNUM")
                    {
                        npc = new NpcDTO();
                        npc.Vnum = short.Parse(linesave[2]);
                        itemAreaBegin = true;
                    }
                    else if (linesave.Length > 2 && linesave[1] == "LEVEL")
                    {
                        if (!itemAreaBegin) continue;
                        npc.Level = byte.Parse(linesave[2]);

                    }
                    else if (linesave.Length > 2 && linesave[1] == "NAME")
                    {
                        if (dictionaryIdLang.ContainsKey(linesave[2]))
                            npc.Name = dictionaryIdLang[linesave[2]];
                        else
                            npc.Name = "";
                    }

                    else if (linesave.Length > 7 && linesave[1] == "ATTRIB")
                    {
                        npc.Element = Convert.ToByte(linesave[2]);
                        npc.ElementRate = Convert.ToInt16(linesave[3]);
                        npc.FireResistance = Convert.ToInt16(linesave[4]);
                        npc.WaterResistance = Convert.ToInt16(linesave[5]);
                        npc.LightResistance = Convert.ToInt16(linesave[6]);
                        npc.DarkResistance = Convert.ToInt16(linesave[7]);
                    }
                    else if (linesave.Length > 8 && linesave[1] == "ZSKILL")
                    {
                        npc.AttackClass = Convert.ToByte(linesave[2]);
                    }
                    else if (linesave.Length > 4 && linesave[1] == "WINFO")
                    {
                        npc.AttackUpgrade = Convert.ToByte(linesave[2]);
                    }
                    else if (linesave.Length > 3 && linesave[1] == "AINFO")
                    {
                        npc.DefenceUpgrade = Convert.ToByte(linesave[2]);

                        npclist.Add(npc);
                        itemAreaBegin = false;
                    }

                }
                npcIdStream.Close();
            }



            int npcCounter = 0;
            short map = 0;

            foreach (string[] linesave in packetList.Where(o => o[0].Equals("in") || o[0].Equals("at")))
            {
                if (linesave.Length > 5 && linesave[0] == "at")
                {
                    map = short.Parse(linesave[2]);
                }
                else if (linesave.Length > 7 && linesave[0] == "in" && linesave[1] == "2")
                {
                    try
                    {
                        NpcDTO npctest = npclist.First(s => s.Vnum == short.Parse(linesave[2]));
                        if (npctest != null)
                        {
                            npctest.MapX = short.Parse(linesave[4]);
                            npctest.MapY = short.Parse(linesave[5]);
                            npctest.MapId = map;
                            npctest.NpcId = short.Parse(linesave[3]);

                            npctest.Position = short.Parse(linesave[6]);
                            npctest.Dialog = short.Parse(linesave[9]);
                            npctest.IsSitting = linesave[13] == "1" ? true : false;

                            if (long.Parse(linesave[3]) >= 10000) continue; // Dialog too high. but why? in order to avoid partners
                            if (DAOFactory.NpcDAO.LoadById(npctest.NpcId) != null || Maps.FirstOrDefault(s=>s.MapId == npctest.MapId) == null) continue; // Npc already existing
                            DAOFactory.NpcDAO.Insert(npctest);
                            npcCounter++;
                        }
                    }
                    catch (Exception)
                    {
                        // Continue with next line in packet file
                    }
                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCS_PARSED"), npcCounter));
        }

        public void ImportShopItems()
        {
            List<PortalDTO> listPacket = new List<PortalDTO>();
            int portalCounter = 0;
            byte type = 0;
            foreach (string[] linesave in packetList.Where(o => o[0].Equals("n_inv") || o[0].Equals("shopping")))
            {
                if (linesave[0].Equals("n_inv"))
                {
                    if (DAOFactory.ShopDAO.LoadByNpc(short.Parse(linesave[2])) != null)
                    {
                        for (int i = 5; i < linesave.Count(); i++)
                        {
                            string[] item = linesave[i].Split('.');
                            ShopItemDTO sitem = null;
                            if (item.Count() == 5)
                            {
                                sitem = new ShopItemDTO();
                                sitem.ShopId = DAOFactory.ShopDAO.LoadByNpc(short.Parse(linesave[2])).ShopId;
                                sitem.Type = type;
                                sitem.Slot = short.Parse(item[1]);
                                sitem.ItemVNum = short.Parse(item[2]);
                                sitem.Gold = long.Parse(item[4]);
                            }
                            else if (item.Count() == 6)
                            {
                                sitem = new ShopItemDTO();
                                sitem.ShopId = DAOFactory.ShopDAO.LoadByNpc(short.Parse(linesave[2])).ShopId;
                                sitem.Type = type;
                                sitem.Slot = short.Parse(item[1]);
                                sitem.ItemVNum = short.Parse(item[2]);
                                sitem.Rare = byte.Parse(item[3]);
                                sitem.Upgrade = byte.Parse(item[4]);
                                sitem.Gold = long.Parse(item[5]);

                            }
                            if (sitem != null && DAOFactory.ShopItemDAO.LoadByShopId(sitem.ShopId).FirstOrDefault(s => s.ItemVNum.Equals(sitem.ItemVNum)) == null)
                            {
                                DAOFactory.ShopItemDAO.Insert(sitem);
                                portalCounter++;
                            }

                        }

                    }

                }
                else
                {
                    if (linesave.Count() > 3)
                        type = byte.Parse(linesave[1]);
                }
            }


            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_PARSED"), portalCounter));

        }

        public void ImportPackets()
        {
            string filePacket = $"{_folder}\\packet.txt";
            using (StreamReader packetTxtStream = new StreamReader(filePacket, Encoding.GetEncoding(1252)))
            {
                string line;
                while ((line = packetTxtStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split(' ');
                    packetList.Add(linesave);
                }
            }
        }

        public void ImportPortals()
        {
            List<PortalDTO> listPacket = new List<PortalDTO>();
            List<PortalDTO> listPortal = new List<PortalDTO>();
            short map = 0;
            int portalCounter = 0;

            foreach (string[] linesave in packetList.Where(o => o[0].Equals("at") || o[0].Equals("gp")))
            {
                if (linesave.Length > 5 && linesave[0] == "at")
                {
                    map = short.Parse(linesave[2]);
                }
                else if (linesave.Length > 4 && linesave[0] == "gp")
                {
                    PortalDTO portal = new PortalDTO
                    {
                        SourceMapId = map,
                        SourceX = short.Parse(linesave[1]),
                        SourceY = short.Parse(linesave[2]),
                        DestinationMapId = short.Parse(linesave[3]),
                        Type = short.Parse(linesave[4]),
                        DestinationX = -1,
                        DestinationY = -1,
                        IsDisabled = false
                    };

                    if (listPacket.FirstOrDefault(s => s.SourceMapId == map && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY && s.DestinationMapId == portal.DestinationMapId) != null || Maps.FirstOrDefault(s => s.MapId == portal.SourceMapId) == null || Maps.FirstOrDefault(s => s.MapId == portal.DestinationMapId) == null)
                        continue; // Portal already in list

                    listPacket.Add(portal);
                }
            }

            listPacket = listPacket.OrderBy(s => s.SourceMapId).ThenBy(s => s.DestinationMapId).ThenBy(s => s.SourceY).ThenBy(s => s.SourceX).ToList();
            foreach (PortalDTO portal in listPacket)
            {
                PortalDTO p = listPacket.Except(listPortal).FirstOrDefault(s => s.SourceMapId.Equals(portal.DestinationMapId) && s.DestinationMapId.Equals(portal.SourceMapId));
                if (p == null) continue;

                portal.DestinationX = p.SourceX;
                portal.DestinationY = p.SourceY;
                p.DestinationY = portal.SourceY;
                p.DestinationX = portal.SourceX;
                listPortal.Add(p);
                listPortal.Add(portal);
            }

            // foreach portal in the new list of Portals
            // where none (=> !Any()) are found in the existing
            foreach (PortalDTO portal in listPortal.Where(portal => !DAOFactory.PortalDAO.LoadByMap(portal.SourceMapId).Any(s => s.DestinationMapId.Equals(portal.DestinationMapId) && s.SourceX.Equals(portal.SourceX) && s.SourceY.Equals(portal.SourceY))))
            {
                // so this dude doesnt exist yet in DAOFactory -> insert it
                DAOFactory.PortalDAO.Insert(portal);
                portalCounter++;
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("PORTALS_PARSED"), portalCounter));
        }

        public void ImportShops()
        {
           int shopCounter = 0;

            foreach (string[] linesave in packetList.Where(o => o[0].Equals("shop")))
            {
                if (linesave.Length > 6 && linesave[0] == "shop" && linesave[1] == "2")
                {
                    NpcDTO npc = DAOFactory.NpcDAO.LoadById(short.Parse(linesave[2])) ;
                    if (npc ==null) continue;

                    string named = "";
                    for (int j = 6; j < linesave.Length; j++)
                    {
                        named += $"{linesave[j]} ";
                    }
                    named = named.Trim();

                    ShopDTO shop = new ShopDTO
                    {
                        Name = named,
                        NpcId = npc.NpcId,
                        MenuType = short.Parse(linesave[4]),
                        ShopType = short.Parse(linesave[5])
                    };
                    if (DAOFactory.ShopDAO.LoadByNpc(shop.NpcId) == null)
                    {
                        DAOFactory.ShopDAO.Insert(shop);
                        shopCounter++;
                    }
                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPS_PARSED"), shopCounter));
        }

        #endregion
    }
}