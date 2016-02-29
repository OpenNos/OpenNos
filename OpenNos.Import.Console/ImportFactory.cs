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

        #endregion

        #region Instantiation

        public ImportFactory(string folder)
        {
            _folder = folder;
        }

        #endregion

        #region Methods

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
                    if (linesave.Length > 1)
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
                    if (linesave.Length > 1)
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
                        item.Price = long.Parse(linesave[3]);
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
                        //item.FashionType = Convert.ToInt16(linesave[4]);
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
                        item.Sex = linesave[17] == "1" ? (byte)2 : (byte)0;
                        item.Sex = linesave[18] == "1" ? (byte)1 : item.Sex;

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
                                item.LevelMinimum = Convert.ToInt16(linesave[2]);
                                item.DamageMinimum = Convert.ToInt16(linesave[3]);
                                item.DamageMaximum = Convert.ToInt16(linesave[4]);
                                item.HitRate = Convert.ToInt16(linesave[5]);
                                item.CriticalLuckRate = Convert.ToInt16(linesave[6]);
                                item.CriticalRate = Convert.ToInt16(linesave[7]);
                                item.BasicUpgrade = Convert.ToByte(linesave[10]);
                                break;
                            case (byte)ItemType.Armor:
                                item.LevelMinimum = Convert.ToInt16(linesave[2]);
                                item.RangeDefence = Convert.ToInt16(linesave[3]);
                                item.DistanceDefence = Convert.ToInt16(linesave[4]);
                                item.MagicDefence = Convert.ToInt16(linesave[5]);
                                item.DefenceDodge = Convert.ToInt16(linesave[6]);
                                item.BasicUpgrade = Convert.ToByte(linesave[10]);
                                break;
                            case (byte)ItemType.Box:
                                item.IsPearl = linesave[2] == "1" ? true : false;
                                //item.PetVnum = Convert.ToInt16(linesave[3]);
                                //item.PetLevel = Convert.ToInt16(linesave[4]);
                                break;
                            case (byte)ItemType.Fashion:
                                item.LevelMinimum = Convert.ToInt16(linesave[2]);
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
                                    item.LevelMinimum = Convert.ToInt16(linesave[2]);
                                    item.MaxCellonLvl = Convert.ToByte(linesave[3]);
                                    item.MaxCellon = Convert.ToByte(linesave[4]);
                                }
                                break;
                            case (byte)ItemType.Magical1:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                break;
                            case (byte)ItemType.Magical2:
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
                                item.LevelJobMinimum = Convert.ToInt16(linesave[20]);
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
                                break;
                            case (byte)ItemType.Shell:
                                //item.ShellMinimumLevel = Convert.ToInt16(linesave[3]);//wtf\/\/ this two things are wrong in many ways
                                //item.ShellMaximumLevel = Convert.ToInt16(linesave[4]);//wtf/\/\ this two things are wrong in many ways
                                //item.ShellType = Convert.ToByte(linesave[5]); //3 shells of each type
                                break;
                            case (byte)ItemType.Main:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                //item.UsableType = Convert.ToByte(linesave[3]);
                                break;
                            case (byte)ItemType.Upgrade:
                                item.Effect = Convert.ToInt16(linesave[2]);
                                item.CellonLvl = Convert.ToByte(linesave[3]);
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
                                //item.PetLoyality = Convert.ToInt16(linesave[4]);
                                //item.PetFood = Convert.ToInt16(linesave[7]);
                                break;
                            case (byte)ItemType.Special:
                                //item.NpcInteraction = Convert.ToInt16(linesave[5]);
                                //item.QuestItem = linesave[7] == "1" ? true : false;
                                break;
                            case (byte)ItemType.Part:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Sell:
                                //nothing to parse
                                break;
                            case (byte)ItemType.Quest:
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
            Dictionary<int, KeyValuePair<string, short>> dictionaryNpcs = new Dictionary<int, KeyValuePair<string, short>>();
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();

            string line;

            int vnum = -1;
            string name2 = "";
            bool itemAreaBegin = false;
            using (StreamReader npcIdStream = new StreamReader(fileNpcId, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');

                    if (linesave.Length > 2 && linesave[1] == "VNUM")
                    {
                        vnum = int.Parse(linesave[2]);
                        itemAreaBegin = true;
                    }
                    else if (linesave.Length > 2 && linesave[1] == "LEVEL")
                    {
                        if (!itemAreaBegin) continue;

                        dictionaryNpcs.Add(vnum, new KeyValuePair<string, short>(name2, short.Parse(linesave[2])));
                        // maybe set 'name2' and 'vnum' to default() for security?
                        itemAreaBegin = false;
                    }
                    else if (linesave.Length > 2 && linesave[1] == "NAME")
                    {
                        name2 = linesave[2];
                    }
                }
                npcIdStream.Close();
            }

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
                        if (long.Parse(linesave[3]) >= 10000)
                            continue; // Dialog too high. but why? in order to avoid partners

                        if (
                            DAOFactory.NpcDAO.LoadFromMap(map)
                                .FirstOrDefault(
                                    s => s.MapId.Equals(map) && s.Vnum.Equals(short.Parse(linesave[2]))) != null)
                            continue; // Npc already existing

                        KeyValuePair<string, short> nameAndLevel = dictionaryNpcs[int.Parse(linesave[2])];
                        DAOFactory.NpcDAO.Insert(new NpcDTO
                        {
                            Vnum = short.Parse(linesave[2]),
                            Level = nameAndLevel.Value,
                            MapId = map,
                            MapX = short.Parse(linesave[4]),
                            MapY = short.Parse(linesave[5]),
                            Name = dictionaryIdLang[nameAndLevel.Key],
                            Position = short.Parse(linesave[6]),
                            Dialog = short.Parse(linesave[9])
                        });
                        npcCounter++;
                    }
                    catch (Exception)
                    {
                        // Continue with next line in packet file
                    }
                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCS_PARSED"), npcCounter));
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

                    if (listPacket.FirstOrDefault(s => s.SourceMapId == map && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY && s.DestinationMapId == portal.DestinationMapId) != null)
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
            Dictionary<int, int> dictionaryId = new Dictionary<int, int>();

            short lastMap = 0; // unused variable
            short currentMap = 0;
            int shopCounter = 0;

            foreach (string[] linesave in packetList.Where(o => o[0].Equals("at") || o[0].Equals("in") || o[0].Equals("shop")))
            {
                if (linesave.Length > 5 && linesave[0] == "at")
                {
                    lastMap = currentMap;
                    currentMap = short.Parse(linesave[2]);
                }
                else if (linesave.Length > 7 && linesave[0] == "in" && linesave[1] == "2")
                {
                    if (long.Parse(linesave[3]) >= 10000) continue;

                    NpcDTO npc = DAOFactory.NpcDAO.LoadFromMap(currentMap).FirstOrDefault(s => s.MapId.Equals(currentMap) && s.Vnum.Equals(short.Parse(linesave[2])));
                    if (npc == null) continue;

                    if (!dictionaryId.ContainsKey(short.Parse(linesave[3])))
                        dictionaryId.Add(short.Parse(linesave[3]), npc.NpcId);
                }
                else if (linesave.Length > 6 && linesave[0] == "shop" && linesave[1] == "2")
                {
                    if (!dictionaryId.ContainsKey(short.Parse(linesave[2]))) continue;

                    string named = "";
                    for (int j = 6; j < linesave.Length; j++)
                    {
                        named += $"{linesave[j]} ";
                    }
                    named = named.Trim();

                    ShopDTO shop = new ShopDTO
                    {
                        Name = named,
                        NpcId = (short)dictionaryId[short.Parse(linesave[2])],
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

        public void ImportShopItems()
        {
            ShopItemDTO shopItem = new ShopItemDTO();

            int shopItemCounter = 0;

            foreach (string[] linesave in packetList.Where(o => o[0].Equals("n_inv")))
            {
                if (linesave.Length > 9 && linesave[0] == "n_inv" && linesave[1] == "2")
                {
                    shopItem.ShopId = short.Parse(linesave[3]);
                    shopItem.Color = short.Parse(linesave[4]);
                    shopItem.Type = byte.Parse(linesave[6]);
                    shopItem.Slot = short.Parse(linesave[7]);
                    shopItem.ItemVNum = short.Parse(linesave[8]);
                    shopItem.Upgrade = byte.Parse(linesave[9]);

                    if (linesave.Length > 10)
                    {
                        shopItem.Rare = byte.Parse(linesave[10]);
                        shopItem.Gold = long.Parse(linesave[11]);
                    }
                    else
                    {
                        shopItem.Gold = long.Parse(linesave[10]);
                    }

                }
                if (DAOFactory.ShopItemDAO.LoadByShopId(shopItem.ShopId) == null)
                {
                    DAOFactory.ShopItemDAO.InsertOrUpdate(ref shopItem);
                    shopItemCounter++;

                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_PARSED"), shopItemCounter));
        }

        #endregion
    }
}