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
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Import.Console
{
    public class ImportFactory
    {
        #region Members

        private readonly string _folder;
        private readonly List<string[]> _packetList = new List<string[]>();
        private List<MapDTO> _maps;

        #endregion

        #region Instantiation

        public ImportFactory(string folder)
        {
            _folder = folder;
        }

        #endregion

        #region Methods

        public static void ImportAccounts()
        {
            AccountDTO acc1 = new AccountDTO
            {
                AccountId = 1,
                Authority = AuthorityType.GameMaster,
                Name = "admin",
                Password = EncryptionBase.Sha512("test")
            };
            DAOFactory.AccountDAO.InsertOrUpdate(ref acc1);

            AccountDTO acc2 = new AccountDTO
            {
                AccountId = 2,
                Authority = AuthorityType.User,
                Name = "test",
                Password = EncryptionBase.Sha512("test")
            };
            DAOFactory.AccountDAO.InsertOrUpdate(ref acc2);
        }

        public void ImportCards()
        {
            string fileCardDat = $"{_folder}\\Card.dat";
            string fileCardLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_Card.txt";
            List<CardDTO> cards = new List<CardDTO>();
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            CardDTO card = new CardDTO();
            BCardDTO bcard;
            List<BCardDTO> bcards = new List<BCardDTO>();
            string line;
            int counter = 0;
            bool itemAreaBegin = false;

            using (StreamReader npcIdLangStream = new StreamReader(fileCardLang, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }
                npcIdLangStream.Close();
            }

            using (StreamReader npcIdStream = new StreamReader(fileCardDat, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 2 && currentLine[1] == "VNUM")
                    {
                        card = new CardDTO
                        {
                            CardId = Convert.ToInt16(currentLine[2])
                        };
                        itemAreaBegin = true;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        card.Name = dictionaryIdLang.ContainsKey(currentLine[2]) ? dictionaryIdLang[currentLine[2]] : string.Empty;
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "GROUP")
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }
                        card.Level = Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "EFFECT")
                    {
                        card.EffectId = Convert.ToInt32(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "STYLE")
                    {
                        card.BuffType = (BuffType)Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "TIME")
                    {
                        card.Duration = Convert.ToInt32(currentLine[2]);
                        card.Delay = Convert.ToInt32(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "1ST")
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (currentLine[2 + i * 6] != "-1" && currentLine[2 + i * 6] != "0")
                            {
                                bcard = new BCardDTO()
                                {
                                    CardId = card.CardId,
                                    Type = Convert.ToByte(currentLine[2 + i * 6]),
                                    SubType = Convert.ToByte(currentLine[3 + i * 6]),
                                    ThirdData = Convert.ToByte(currentLine[5 + i * 6]),
                                    FirstData = Convert.ToInt32(currentLine[6 + i * 6]) / 4,
                                    SecondData = Convert.ToInt32(currentLine[7 + i * 6]) / 4
                                };
                                bcards.Add(bcard);
                            }
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "2ST")
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (currentLine[2 + i * 6] != "0" && currentLine[2 + i * 6] != "-1")
                            {
                                bcard = new BCardDTO()
                                {
                                    CastType = 1,
                                    CardId = card.CardId,
                                    Type = byte.Parse(currentLine[2+i * 6]),
                                    SubType = (byte)((int.Parse(currentLine[4]) + 1) * 10),
                                    FirstData = (short)(int.Parse(currentLine[5]) / 4),
                                    SecondData = (short)(int.Parse(currentLine[6]) / 4),
                                    ThirdData = (short)(int.Parse(currentLine[7]) / 4),
                                };
                                bcards.Add(bcard);
                            }
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "LAST")
                    {
                        card.TimeoutBuff = Convert.ToInt16(currentLine[2]);
                        card.TimeoutBuffChance = Convert.ToByte(currentLine[3]);

                        // investigate
                        if (DAOFactory.CardDAO.LoadById(card.CardId) == null)
                        {
                            cards.Add(card);
                            counter++;
                        }
                        itemAreaBegin = false;
                    }
                }
                DAOFactory.CardDAO.Insert(cards);
                DAOFactory.BCardDAO.Insert(bcards);

                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("CARDS_PARSED"), counter));
                npcIdStream.Close();
            }
        }


        public void ImportMapNpcs()
        {
            int npcCounter = 0;
            short map = 0;
            List<MapNpcDTO> npcs = new List<MapNpcDTO>();
            List<int> npcMvPacketsList = new List<int>();
            Dictionary<int, short> effPacketsDictionary = new Dictionary<int, short>();

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("mv") && o[1].Equals("2")))
            {
                if (long.Parse(currentPacket[2]) >= 20000)
                {
                    continue;
                }
                if (!npcMvPacketsList.Contains(Convert.ToInt32(currentPacket[2])))
                {
                    npcMvPacketsList.Add(Convert.ToInt32(currentPacket[2]));
                }
            }

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("eff") && o[1].Equals("2")))
            {
                if (long.Parse(currentPacket[2]) >= 20000)
                {
                    continue;
                }
                if (!effPacketsDictionary.ContainsKey(Convert.ToInt32(currentPacket[2])))
                {
                    effPacketsDictionary.Add(Convert.ToInt32(currentPacket[2]), Convert.ToInt16(currentPacket[3]));
                }
            }

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("in") || o[0].Equals("at")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    continue;
                }
                if (currentPacket.Length > 7 && currentPacket[0] == "in" && currentPacket[1] == "2")
                {
                    MapNpcDTO npctest = new MapNpcDTO
                    {
                        MapX = short.Parse(currentPacket[4]),
                        MapY = short.Parse(currentPacket[5]),
                        MapId = map,
                        NpcVNum = short.Parse(currentPacket[2])
                    };
                    if (long.Parse(currentPacket[3]) > 20000)
                    {
                        continue;
                    }
                    npctest.MapNpcId = short.Parse(currentPacket[3]);
                    if (effPacketsDictionary.ContainsKey(npctest.MapNpcId))
                    {
                        npctest.Effect = effPacketsDictionary[npctest.MapNpcId];
                    }
                    npctest.EffectDelay = 4750;
                    npctest.IsMoving = npcMvPacketsList.Contains(npctest.MapNpcId);
                    npctest.Position = byte.Parse(currentPacket[6]);
                    npctest.Dialog = short.Parse(currentPacket[9]);
                    npctest.IsSitting = currentPacket[13] != "1";
                    npctest.IsDisabled = false;

                    if (DAOFactory.NpcMonsterDAO.LoadByVNum(npctest.NpcVNum) == null || DAOFactory.MapNpcDAO.LoadById(npctest.MapNpcId) != null || npcs.Count(i => i.MapNpcId == npctest.MapNpcId) != 0)
                    {
                        continue;
                    }

                    npcs.Add(npctest);
                    npcCounter++;
                }
            }
            DAOFactory.MapNpcDAO.Insert(npcs);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCS_PARSED"), npcCounter));
        }

        public void ImportMaps()
        {
            string fileMapIdDat = $"{_folder}\\MapIDData.dat";
            string fileMapIdLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_MapIDData.txt";
            string folderMap = $"{_folder}\\map";
            List<MapDTO> maps = new List<MapDTO>();
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
                    if (linesave.Length <= 1)
                    {
                        continue;
                    }
                    if (!int.TryParse(linesave[0], out int mapid))
                    {
                        continue;
                    }
                    if (!dictionaryId.ContainsKey(mapid))
                    {
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
                    if (linesave.Length <= 1 || dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        continue;
                    }
                    dictionaryIdLang.Add(linesave[0], linesave[1]);
                }
                mapIdLangStream.Close();
            }

            foreach (string[] linesave in _packetList.Where(o => o[0].Equals("at")))
            {
                if (linesave.Length <= 7 || linesave[0] != "at")
                {
                    continue;
                }
                if (dictionaryMusic.ContainsKey(int.Parse(linesave[2])))
                {
                    continue;
                }
                dictionaryMusic.Add(int.Parse(linesave[2]), int.Parse(linesave[7]));
            }

            foreach (FileInfo file in new DirectoryInfo(folderMap).GetFiles())
            {
                string name = string.Empty;
                int music = 0;

                if (dictionaryId.ContainsKey(int.Parse(file.Name)) && dictionaryIdLang.ContainsKey(dictionaryId[int.Parse(file.Name)]))
                {
                    name = dictionaryIdLang[dictionaryId[int.Parse(file.Name)]];
                }
                if (dictionaryMusic.ContainsKey(int.Parse(file.Name)))
                {
                    music = dictionaryMusic[int.Parse(file.Name)];
                }
                MapDTO map = new MapDTO
                {
                    Name = name,
                    Music = music,
                    MapId = short.Parse(file.Name),
                    Data = File.ReadAllBytes(file.FullName),
                    ShopAllowed = short.Parse(file.Name) == 147
                };
                if (DAOFactory.MapDAO.LoadById(map.MapId) != null)
                {
                    continue; // Map already exists in list
                }
                maps.Add(map);
                i++;
            }
            DAOFactory.MapDAO.Insert(maps);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPS_PARSED"), i));
        }

        public void ImportMapType()
        {
            List<MapTypeDTO> list = DAOFactory.MapTypeDAO.LoadAll().ToList();
            MapTypeDTO mt1 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act1,
                MapTypeName = "Act1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt1.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt1);
            }
            MapTypeDTO mt2 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act2,
                MapTypeName = "Act2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt2.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt2);
            }
            MapTypeDTO mt3 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act3,
                MapTypeName = "Act3",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt3.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt3);
            }
            MapTypeDTO mt4 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act4,
                MapTypeName = "Act4",
                PotionDelay = 5000
            };
            if (list.All(s => s.MapTypeId != mt4.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt4);
            }
            MapTypeDTO mt5 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act51,
                MapTypeName = "Act5.1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct5,
                ReturnMapTypeId = (long)RespawnType.ReturnAct5
            };
            if (list.All(s => s.MapTypeId != mt5.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt5);
            }
            MapTypeDTO mt6 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act52,
                MapTypeName = "Act5.2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct5,
                ReturnMapTypeId = (long)RespawnType.ReturnAct5
            };
            if (list.All(s => s.MapTypeId != mt6.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt6);
            }
            MapTypeDTO mt7 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act61,
                MapTypeName = "Act6.1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt7.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt7);
            }
            MapTypeDTO mt8 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act62,
                MapTypeName = "Act6.2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt8.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt8);
            }
            MapTypeDTO mt9 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act61a,
                MapTypeName = "Act6.1a", // angel camp
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt9.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt9);
            }
            MapTypeDTO mt10 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act61d,
                MapTypeName = "Act6.1d", // demon camp
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt10.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt10);
            }
            MapTypeDTO mt11 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.CometPlain,
                MapTypeName = "CometPlain",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt11.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt11);
            }
            MapTypeDTO mt12 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Mine1,
                MapTypeName = "Mine1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt12.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt12);
            }
            MapTypeDTO mt13 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Mine2,
                MapTypeName = "Mine2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt13.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt13);
            }
            MapTypeDTO mt14 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.MeadowOfMine,
                MapTypeName = "MeadownOfPlain",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt14.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt14);
            }
            MapTypeDTO mt15 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.SunnyPlain,
                MapTypeName = "SunnyPlain",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt15.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt15);
            }
            MapTypeDTO mt16 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Fernon,
                MapTypeName = "Fernon",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt16.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt16);
            }
            MapTypeDTO mt17 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.FernonF,
                MapTypeName = "FernonF",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt17.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt17);
            }
            MapTypeDTO mt18 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Cliff,
                MapTypeName = "Cliff",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt18.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt18);
            }
            MapTypeDTO mt19 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.LandOfTheDead,
                MapTypeName = "LandOfTheDead",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt19.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt19);
            }
            MapTypeDTO mt20 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act32,
                MapTypeName = "Act 3.2",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt20.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt20);
            }
            MapTypeDTO mt21 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.CleftOfDarkness,
                MapTypeName = "Cleft of Darkness",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt21.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt21);
            }
            MapTypeDTO mt22 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.PVPMap,
                MapTypeName = "PVPMap",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt22.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt22);
            }
            MapTypeDTO mt23 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Citadel,
                MapTypeName = "Citadel",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt23.MapTypeId))
            {
                DAOFactory.MapTypeDAO.Insert(ref mt23);
            }
            Logger.Log.Info(Language.Instance.GetMessageFromKey("MAPTYPES_PARSED"));
        }

        public void ImportMapTypeMap()
        {
            List<MapTypeMapDTO> maptypemaps = new List<MapTypeMapDTO>();
            short mapTypeId = 1;
            for (int i = 1; i < 300; i++)
            {
                bool objectset = false;
                if (i < 3 || i > 48 && i < 53 || i > 67 && i < 76 || i == 102 || i > 103 && i < 105 || i > 144 && i < 149)
                {
                    // "act1"
                    mapTypeId = (short)MapTypeEnum.Act1;
                    objectset = true;
                }
                else if (i > 19 && i < 34 || i > 52 && i < 68 || i > 84 && i < 101)
                {
                    // "act2"
                    mapTypeId = (short)MapTypeEnum.Act2;
                    objectset = true;
                }
                else if (i > 40 && i < 45 || i > 45 && i < 48 || i > 99 && i < 102 || i > 104 && i < 128)
                {
                    // "act3"
                    mapTypeId = (short)MapTypeEnum.Act3;
                    objectset = true;
                }
                else if (i == 260)
                {
                    // "act3.2"
                    mapTypeId = (short)MapTypeEnum.Act32;
                    objectset = true;
                }
                else if (i > 129 && i <= 134 || i == 135 || i == 137 || i == 139 || i == 141 || i > 150 && i < 155)
                {
                    // "act4"
                    mapTypeId = (short)MapTypeEnum.Act4;
                    objectset = true;
                }
                else if (i > 169 && i < 205)
                {
                    // "act5.1"
                    mapTypeId = (short)MapTypeEnum.Act51;
                    objectset = true;
                }
                else if (i > 204 && i < 221)
                {
                    // "act5.2"
                    mapTypeId = (short)MapTypeEnum.Act52;
                    objectset = true;
                }
                else if (i > 227 && i < 241)
                {
                    // "act6.1"
                    mapTypeId = (short)MapTypeEnum.Act61;
                    objectset = true;
                }
                else if (i > 239 && i < 251 || i == 299)
                {
                    // "act6.2"
                    mapTypeId = (short)MapTypeEnum.Act62;
                    objectset = true;
                }
                else if (i == 103)
                {
                    // "Comet plain"
                    mapTypeId = (short)MapTypeEnum.CometPlain;
                    objectset = true;
                }
                else if (i == 6)
                {
                    // "Mine1"
                    mapTypeId = (short)MapTypeEnum.Mine1;
                    objectset = true;
                }
                else if (i > 6 && i < 9)
                {
                    // "Mine2"
                    mapTypeId = (short)MapTypeEnum.Mine2;
                    objectset = true;
                }
                else if (i == 3)
                {
                    // "Meadown of mine"
                    mapTypeId = (short)MapTypeEnum.MeadowOfMine;
                    objectset = true;
                }
                else if (i == 4)
                {
                    // "Sunny plain"
                    mapTypeId = (short)MapTypeEnum.SunnyPlain;
                    objectset = true;
                }
                else if (i == 5)
                {
                    // "Fernon"
                    mapTypeId = (short)MapTypeEnum.Fernon;
                    objectset = true;
                }
                else if (i > 9 && i < 19 || i > 79 && i < 85)
                {
                    // "FernonF"
                    mapTypeId = (short)MapTypeEnum.FernonF;
                    objectset = true;
                }
                else if (i > 75 && i < 79)
                {
                    // "Cliff"
                    mapTypeId = (short)MapTypeEnum.Cliff;
                    objectset = true;
                }
                else if (i == 150)
                {
                    // "Land of the dead"
                    mapTypeId = (short)MapTypeEnum.LandOfTheDead;
                    objectset = true;
                }
                else if (i == 138)
                {
                    // "Cleft of Darkness"
                    mapTypeId = (short)MapTypeEnum.CleftOfDarkness;
                    objectset = true;
                }
                else if (i == 9305)
                {
                    // "PVPMap"
                    mapTypeId = (short)MapTypeEnum.PVPMap;
                    objectset = true;
                }
                else if (i == 130 && i == 131)
                {
                    // "Citadel"
                    mapTypeId = (short)MapTypeEnum.Citadel;
                    objectset = true;
                }

                // add "act6.1a" and "act6.1d" when ids found
                if (objectset && DAOFactory.MapDAO.LoadById((short)i) != null && DAOFactory.MapTypeMapDAO.LoadByMapAndMapType((short)i, mapTypeId) == null)
                {
                    maptypemaps.Add(new MapTypeMapDTO { MapId = (short)i, MapTypeId = mapTypeId });
                }
            }
            DAOFactory.MapTypeMapDAO.Insert(maptypemaps);
        }

        public void ImportMonsters()
        {
            int monsterCounter = 0;
            short map = 0;
            List<int> mobMvPacketsList = new List<int>();
            List<MapMonsterDTO> monsters = new List<MapMonsterDTO>();

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("mv") && o[1].Equals("3")))
            {
                if (!mobMvPacketsList.Contains(Convert.ToInt32(currentPacket[2])))
                {
                    mobMvPacketsList.Add(Convert.ToInt32(currentPacket[2]));
                }
            }

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("in") || o[0].Equals("at")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    continue;
                }
                if (currentPacket.Length > 7 && currentPacket[0] == "in" && currentPacket[1] == "3")
                {
                    MapMonsterDTO monster = new MapMonsterDTO
                    {
                        MapId = map,
                        MonsterVNum = short.Parse(currentPacket[2]),
                        MapMonsterId = int.Parse(currentPacket[3]),
                        MapX = short.Parse(currentPacket[4]),
                        MapY = short.Parse(currentPacket[5]),
                        Position = (byte)(currentPacket[6] == string.Empty ? 0 : byte.Parse(currentPacket[6])),
                        IsDisabled = false
                    };
                    monster.IsMoving = mobMvPacketsList.Contains(monster.MapMonsterId);

                    if (DAOFactory.NpcMonsterDAO.LoadByVNum(monster.MonsterVNum) == null || DAOFactory.MapMonsterDAO.LoadById(monster.MapMonsterId) != null || monsters.Count(i => i.MapMonsterId == monster.MapMonsterId) != 0)
                    {
                        continue;
                    }

                    monsters.Add(monster);
                    monsterCounter++;
                }
            }

            DAOFactory.MapMonsterDAO.Insert(monsters);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MONSTERS_PARSED"), monsterCounter));
        }

        public void ImportNpcMonsterData()
        {
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("e_info") && o[1].Equals("10")))
            {
                if (currentPacket.Length > 25)
                {
                    NpcMonsterDTO npcMonster = DAOFactory.NpcMonsterDAO.LoadByVNum(short.Parse(currentPacket[2]));
                    if (npcMonster == null)
                    {
                        continue;
                    }
                    npcMonster.AttackClass = byte.Parse(currentPacket[5]);
                    npcMonster.AttackUpgrade = byte.Parse(currentPacket[7]);
                    npcMonster.DamageMinimum = short.Parse(currentPacket[8]);
                    npcMonster.DamageMaximum = short.Parse(currentPacket[9]);
                    npcMonster.Concentrate = short.Parse(currentPacket[10]);
                    npcMonster.CriticalChance = byte.Parse(currentPacket[11]);
                    npcMonster.CriticalRate = short.Parse(currentPacket[12]);
                    npcMonster.DefenceUpgrade = byte.Parse(currentPacket[13]);
                    npcMonster.CloseDefence = short.Parse(currentPacket[14]);
                    npcMonster.DefenceDodge = short.Parse(currentPacket[15]);
                    npcMonster.DistanceDefence = short.Parse(currentPacket[16]);
                    npcMonster.DistanceDefenceDodge = short.Parse(currentPacket[17]);
                    npcMonster.MagicDefence = short.Parse(currentPacket[18]);
                    npcMonster.FireResistance = sbyte.Parse(currentPacket[19]);
                    npcMonster.WaterResistance = sbyte.Parse(currentPacket[20]);
                    npcMonster.LightResistance = sbyte.Parse(currentPacket[21]);
                    npcMonster.DarkResistance = sbyte.Parse(currentPacket[22]);

                    // TODO: BCard Buff parsing
                    DAOFactory.NpcMonsterDAO.InsertOrUpdate(ref npcMonster);
                }
            }
        }

        public void ImportNpcMonsters()
        {
            int[] basicHp = new int[100];
            int[] basicMp = new int[100];
            int[] basicXp = new int[100];
            int[] basicJXp = new int[100];

            // basicHpLoad
            int baseHp = 138;
            int basup = 17;
            for (int i = 0; i < 100; i++)
            {
                basicHp[i] = baseHp;
                basup++;
                baseHp += basup;

                if (i == 37)
                {
                    baseHp = 1765;
                    basup = 65;
                }
                if (i >= 41)
                {
                    if ((99 - i) % 8 == 0)
                    {
                        basup++;
                    }
                }
            }

            // basicMpLoad
            for (int i = 0; i < 100; i++)
            {
                basicMp[i] = basicHp[i];
            }

            // basicXPLoad
            for (int i = 0; i < 100; i++)
            {
                basicXp[i] = i * 180;
            }

            // basicJXpLoad
            for (int i = 0; i < 100; i++)
            {
                basicJXp[i] = 360;
            }

            string fileNpcId = $"{_folder}\\monster.dat";
            string fileNpcLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_monster.txt";
            List<NpcMonsterDTO> npcs = new List<NpcMonsterDTO>();

            // Store like this: (vnum, (name, level))
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            NpcMonsterDTO npc = new NpcMonsterDTO();
            List<DropDTO> drops = new List<DropDTO>();
            List<BCardDTO> monstercards = new List<BCardDTO>();
            List<NpcMonsterSkillDTO> skills = new List<NpcMonsterSkillDTO>();
            string line;
            bool itemAreaBegin = false;
            int counter = 0;
            long unknownData = 0;
            using (StreamReader npcIdLangStream = new StreamReader(fileNpcLang, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }
                npcIdLangStream.Close();
            }
            using (StreamReader npcIdStream = new StreamReader(fileNpcId, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 2 && currentLine[1] == "VNUM")
                    {
                        npc = new NpcMonsterDTO
                        {
                            NpcMonsterVNum = Convert.ToInt16(currentLine[2])
                        };
                        itemAreaBegin = true;
                        unknownData = 0;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        npc.Name = dictionaryIdLang.ContainsKey(currentLine[2]) ? dictionaryIdLang[currentLine[2]] : string.Empty;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "LEVEL")
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }
                        npc.Level = Convert.ToByte(currentLine[2]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "RACE")
                    {
                        npc.Race = Convert.ToByte(currentLine[2]);
                        npc.RaceType = Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "ATTRIB")
                    {
                        npc.Element = Convert.ToByte(currentLine[2]);
                        npc.ElementRate = Convert.ToInt16(currentLine[3]);
                        npc.FireResistance = Convert.ToSByte(currentLine[4]);
                        npc.WaterResistance = Convert.ToSByte(currentLine[5]);
                        npc.LightResistance = Convert.ToSByte(currentLine[6]);
                        npc.DarkResistance = Convert.ToSByte(currentLine[7]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "HP/MP")
                    {
                        npc.MaxHP = Convert.ToInt32(currentLine[2]) + basicHp[npc.Level];
                        npc.MaxMP = Convert.ToInt32(currentLine[3]) + basicMp[npc.Level];
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "EXP")
                    {
                        npc.XP = Math.Abs(Convert.ToInt32(currentLine[2]) + basicXp[npc.Level]);
                        npc.JobXP = Convert.ToInt32(currentLine[3]) + basicJXp[npc.Level];
                        switch (npc.NpcMonsterVNum)
                        {
                            case 2500:
                                npc.HeroXp = 533;
                                break;

                            case 2501:
                                npc.HeroXp = 534;
                                break;

                            case 2502:
                                npc.HeroXp = 535;
                                break;

                            case 2503:
                                npc.HeroXp = 614;
                                break;

                            case 2510:
                                npc.HeroXp = 534;
                                break;

                            case 2511:
                                npc.HeroXp = 533;
                                break;

                            case 2512:
                                npc.HeroXp = 535;
                                break;

                            case 2513:
                                npc.HeroXp = 651;
                                break;

                            case 2521:
                                npc.HeroXp = 170;
                                break;

                            case 2522:
                                npc.HeroXp = 286;
                                break;

                            case 2523:
                                npc.HeroXp = 328;
                                break;

                            case 2525:
                                npc.HeroXp = 261;
                                break;

                            default:
                                npc.HeroXp = 0;
                                break;
                        }
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "PREATT")
                    {
                        npc.IsHostile = currentLine[2] != "0";
                        npc.NoticeRange = Convert.ToByte(currentLine[4]);
                        npc.Speed = Convert.ToByte(currentLine[5]);
                        npc.RespawnTime = Convert.ToInt32(currentLine[6]);
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "WEAPON")
                    {
                        if (currentLine[3] == "1")
                        {
                            npc.DamageMinimum = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 4 + 32 + Convert.ToInt16(currentLine[4]) + Math.Round(Convert.ToDecimal((npc.Level - 1) / 5)));
                            npc.DamageMaximum = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 6 + 40 + Convert.ToInt16(currentLine[5]) - Math.Round(Convert.ToDecimal((npc.Level - 1) / 5)));
                            npc.Concentrate = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 5 + 27 + Convert.ToInt16(currentLine[6]));
                            npc.CriticalChance = Convert.ToByte(4 + Convert.ToInt16(currentLine[7]));
                            npc.CriticalRate = Convert.ToInt16(70 + Convert.ToInt16(currentLine[8]));
                        }
                        else if (currentLine[3] == "2")
                        {
                            npc.DamageMinimum = Convert.ToInt16(Convert.ToInt16(currentLine[2]) * 6.5f + 23 + Convert.ToInt16(currentLine[4]));
                            npc.DamageMaximum = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 8 + 38 + Convert.ToInt16(currentLine[5]));
                            npc.Concentrate = Convert.ToInt16(70 + Convert.ToInt16(currentLine[6]));
                        }
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "ARMOR")
                    {
                        npc.CloseDefence = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 2 + 18);
                        npc.DistanceDefence = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 3 + 17);
                        npc.MagicDefence = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 2 + 13);
                        npc.DefenceDodge = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 5 + 31);
                        npc.DistanceDefenceDodge = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 5 + 31);
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "ETC")
                    {
                        unknownData = Convert.ToInt64(currentLine[2]);
                        if (unknownData == -2147481593)
                        {
                            npc.MonsterType = MonsterType.Special;
                        }
                        if (unknownData == -2147483616 || unknownData == -2147483647 || unknownData == -2147483646)
                        {
                            if (npc.Race == 8 && npc.RaceType == 0)
                            {
                                npc.NoAggresiveIcon = true;
                            }
                            else
                            {
                                npc.NoAggresiveIcon = false;
                            }
                        }
                        if (npc.NpcMonsterVNum >= 588 && npc.NpcMonsterVNum <= 607)
                        {
                            npc.MonsterType = MonsterType.Elite;
                        }
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "SETTING")
                    {
                        if (currentLine[4] != "0")
                        {
                            npc.VNumRequired = Convert.ToInt16(currentLine[4]);
                            npc.AmountRequired = 1;
                        }
                    }
                    else if (currentLine.Length > 4 && currentLine[1] == "PETINFO")
                    {
                        if (npc.VNumRequired == 0 && (unknownData == -2147481593 || unknownData == -2147481599 || unknownData == -1610610681))
                        {
                            npc.VNumRequired = Convert.ToInt16(currentLine[2]);
                            npc.AmountRequired = Convert.ToByte(currentLine[3]);
                        }
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "EFF")
                    {
                        npc.BasicSkill = Convert.ToInt16(currentLine[2]);
                    }
                    else if (currentLine.Length > 8 && currentLine[1] == "ZSKILL")
                    {
                        npc.AttackClass = Convert.ToByte(currentLine[2]);
                        npc.BasicRange = Convert.ToByte(currentLine[3]);
                        npc.BasicArea = Convert.ToByte(currentLine[5]);
                        npc.BasicCooldown = Convert.ToInt16(currentLine[6]);
                    }
                    else if (currentLine.Length > 4 && currentLine[1] == "WINFO")
                    {
                        npc.AttackUpgrade = Convert.ToByte(unknownData == 1 ? currentLine[2] : currentLine[4]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "AINFO")
                    {
                        npc.DefenceUpgrade = Convert.ToByte(unknownData == 1 ? currentLine[2] : currentLine[3]);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "SKILL")
                    {
                        for (int i = 2; i < currentLine.Length - 3; i += 3)
                        {
                            short vnum = short.Parse(currentLine[i]);
                            if (vnum == -1 || vnum == 0)
                            {
                                break;
                            }
                            if (DAOFactory.SkillDAO.LoadById(vnum) == null || DAOFactory.NpcMonsterSkillDAO.LoadByNpcMonster(npc.NpcMonsterVNum).Count(s => s.SkillVNum == vnum) != 0)
                            {
                                continue;
                            }
                            skills.Add(new NpcMonsterSkillDTO
                            {
                                SkillVNum = vnum,
                                Rate = Convert.ToInt16(currentLine[i + 1]),
                                NpcMonsterVNum = npc.NpcMonsterVNum
                            });
                        }
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "CARD")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            byte type = (byte)(Int32.Parse(currentLine[2 + 5 * i]));
                            if (type != 0)
                            {
                                BCardDTO itemCard = new BCardDTO
                                {
                                    NpcMonsterVNum = npc.NpcMonsterVNum,
                                    Type = type,
                                    SubType = (byte)((int.Parse(currentLine[5 + 5 * i]) + 1) * 10),
                                    FirstData = (short)(int.Parse(currentLine[3 + 5 * i]) / 4),
                                    SecondData = (short)(int.Parse(currentLine[4 + 5 * i]) / 4),
                                    ThirdData = (short)(int.Parse(currentLine[6 + 5 * i]) / 4),
                                };
                                monstercards.Add(itemCard);
                            }
                        }
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "BASIC")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            byte type = (byte)(Int32.Parse(currentLine[2 + 5 * i]));
                            if (type != 0)
                            {
                                BCardDTO itemCard = new BCardDTO
                                {
                                    NpcMonsterVNum = npc.NpcMonsterVNum,
                                    Type = type,
                                    SubType = (byte)((int.Parse(currentLine[6 + 5 * i])) * 10),
                                    FirstData = (short)(int.Parse(currentLine[5 + 5])),
                                    SecondData = (short)(int.Parse(currentLine[4 + 5 * i]) / 4), 
                                    ThirdData = (short)(int.Parse(currentLine[3 + 5 * i]) / 4),
                                    CastType = 1
                                };
                                monstercards.Add(itemCard);
                            }
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "ITEM")
                    {
                        if (DAOFactory.NpcMonsterDAO.LoadByVNum(npc.NpcMonsterVNum) == null)
                        {
                            npcs.Add(npc);
                            counter++;
                        }
                        for (int i = 2; i < currentLine.Length - 3; i += 3)
                        {
                            short vnum = Convert.ToInt16(currentLine[i]);
                            if (vnum == -1)
                            {
                                break;
                            }
                            if (DAOFactory.DropDAO.LoadByMonster(npc.NpcMonsterVNum).Count(s => s.ItemVNum == vnum) != 0)
                            {
                                continue;
                            }
                            drops.Add(new DropDTO
                            {
                                ItemVNum = vnum,
                                Amount = Convert.ToInt32(currentLine[i + 2]),
                                MonsterVNum = npc.NpcMonsterVNum,
                                DropChance = Convert.ToInt32(currentLine[i + 1])
                            });
                        }
                        itemAreaBegin = false;
                    }
                }
                DAOFactory.NpcMonsterDAO.Insert(npcs);
                DAOFactory.NpcMonsterSkillDAO.Insert(skills);
                DAOFactory.BCardDAO.Insert(monstercards);
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_PARSED"), counter));
                npcIdStream.Close();
            }

            // Act 1
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 12000, MapTypeId = (short)MapTypeEnum.Act1 });

            // Act2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 7000, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1028, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 250, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act2 });

            // Act3
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1235, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1238, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1240, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 4500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 350, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act3 });

            // Act3.2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1235, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1238, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1240, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 4500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 350, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act32 });

            // Act4
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 3, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 2, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 3, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 3, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1246, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1247, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1248, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1429, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 2307, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 2308, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });

            // Act5
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 6000, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1872, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1873, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1874, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2351, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2379, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act51 });

            // Act5.2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 5000, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2379, Amount = 1, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2380, Amount = 1, MonsterVNum = null, DropChance = 6000, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act52 });

            // Act6.1
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 5000, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1028, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2803, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2804, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2805, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2806, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2807, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61 });

            // drops.Add(new DropDTO { ItemVNum = 2815, Amount = 1, MonsterVNum = null, DropChance =
            // 450, MapTypeId = 9 }); //Only for angel camp need group act6.1 angel
            drops.Add(new DropDTO { ItemVNum = 2816, Amount = 1, MonsterVNum = null, DropChance = 350, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2818, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 2819, Amount = 1, MonsterVNum = null, DropChance = 350, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act61 });

            // drops.Add(new DropDTO { ItemVNum = 5881, Amount = 1, MonsterVNum = null, DropChance =
            // 450, MapTypeId = 10 }); //Only for demon camp need group act6.1 demon

            // Act6.2 (need some information) > soon )

            // Comet plain
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 7000, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.CometPlain });

            // Mine1
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine1 });
            drops.Add(new DropDTO { ItemVNum = 1005, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine1 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 11000, MapTypeId = (short)MapTypeEnum.Mine1 });

            // Mine2
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 1005, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 11000, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Mine2 });

            // MeadownOfMine
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 1005, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 10000, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2016, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2023, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.MeadowOfMine });

            // SunnyPlain
            drops.Add(new DropDTO { ItemVNum = 1003, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1006, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.SunnyPlain });

            // Fernon
            drops.Add(new DropDTO { ItemVNum = 1003, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1006, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 9000, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Fernon });

            // FernonF
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 9000, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.FernonF });

            // Cliff
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Cliff });

            // LandOfTheDead
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1015, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1016, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1019, Amount = 1, MonsterVNum = null, DropChance = 2000, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1020, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1021, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1022, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1211, Amount = 1, MonsterVNum = null, DropChance = 250, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.LandOfTheDead });

            DAOFactory.DropDAO.Insert(drops);
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
                    _packetList.Add(linesave);
                }
            }
        }

        public void ImportPortals()
        {
            List<PortalDTO> listPortals1 = new List<PortalDTO>();
            List<PortalDTO> listPortals2 = new List<PortalDTO>();
            short map = 0;

            PortalDTO lodPortal = new PortalDTO
            {
                SourceMapId = 150,
                SourceX = 172,
                SourceY = 171,
                DestinationMapId = 98,
                Type = -1,
                DestinationX = 6,
                DestinationY = 36,
                IsDisabled = false
            };
            DAOFactory.PortalDAO.Insert(lodPortal);

            PortalDTO minilandPortal = new PortalDTO
            {
                SourceMapId = 20001,
                SourceX = 3,
                SourceY = 8,
                DestinationMapId = 1,
                Type = -1,
                DestinationX = 48,
                DestinationY = 132,
                IsDisabled = false
            };
            DAOFactory.PortalDAO.Insert(minilandPortal);

            PortalDTO weddingPortal = new PortalDTO
            {
                SourceMapId = 2586,
                SourceX = 34,
                SourceY = 54,
                DestinationMapId = 145,
                Type = -1,
                DestinationX = 61,
                DestinationY = 165,
                IsDisabled = false
            };
            DAOFactory.PortalDAO.Insert(weddingPortal);

            PortalDTO glacerusCavernPortal = new PortalDTO
            {
                SourceMapId = 2587,
                SourceX = 42,
                SourceY = 3,
                DestinationMapId = 189,
                Type = -1,
                DestinationX = 48,
                DestinationY = 156,
                IsDisabled = false
            };
            DAOFactory.PortalDAO.Insert(glacerusCavernPortal);

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("at") || o[0].Equals("gp")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    continue;
                }
                if (currentPacket.Length > 4 && currentPacket[0] == "gp")
                {
                    PortalDTO portal = new PortalDTO
                    {
                        SourceMapId = map,
                        SourceX = short.Parse(currentPacket[1]),
                        SourceY = short.Parse(currentPacket[2]),
                        DestinationMapId = short.Parse(currentPacket[3]),
                        Type = sbyte.Parse(currentPacket[4]),
                        DestinationX = -1,
                        DestinationY = -1,
                        IsDisabled = false
                    };

                    if (listPortals1.Any(s => s.SourceMapId == map && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY && s.DestinationMapId == portal.DestinationMapId) || _maps.All(s => s.MapId != portal.SourceMapId) || _maps.All(s => s.MapId != portal.DestinationMapId))
                    {
                        // Portal already in list
                        continue;
                    }

                    listPortals1.Add(portal);
                }
            }

            listPortals1 = listPortals1.OrderBy(s => s.SourceMapId).ThenBy(s => s.DestinationMapId).ThenBy(s => s.SourceY).ThenBy(s => s.SourceX).ToList();
            foreach (PortalDTO portal in listPortals1)
            {
                PortalDTO p = listPortals1.Except(listPortals2).FirstOrDefault(s => s.SourceMapId == portal.DestinationMapId && s.DestinationMapId == portal.SourceMapId);
                if (p == null)
                {
                    continue;
                }

                portal.DestinationX = p.SourceX;
                portal.DestinationY = p.SourceY;
                p.DestinationY = portal.SourceY;
                p.DestinationX = portal.SourceX;
                listPortals2.Add(p);
                listPortals2.Add(portal);
            }

            // foreach portal in the new list of Portals where none (=> !Any()) are found in the existing
            int portalCounter = listPortals2.Count(portal => !DAOFactory.PortalDAO.LoadByMap(portal.SourceMapId).Any(
                s => s.DestinationMapId == portal.DestinationMapId && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY));

            // so this dude doesnt exist yet in DAOFactory -> insert it
            DAOFactory.PortalDAO.Insert(listPortals2.Where(portal => !DAOFactory.PortalDAO.LoadByMap(portal.SourceMapId).Any(
                s => s.DestinationMapId == portal.DestinationMapId && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY)).ToList());

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("PORTALS_PARSED"), portalCounter));
        }

        public void ImportRecipe()
        {
            int count = 0;
            int mapnpcid = 0;
            short item = 0;
            RecipeDTO recipe;

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("n_run") || o[0].Equals("pdtse") || o[0].Equals("m_list")))
            {
                if (currentPacket.Length > 4 && currentPacket[0] == "n_run")
                {
                    int.TryParse(currentPacket[4], out mapnpcid);
                    continue;
                }
                if (currentPacket.Length > 1 && currentPacket[0] == "m_list" && (currentPacket[1] == "2" || currentPacket[1] == "4"))
                {
                    for (int i = 2; i < currentPacket.Length - 1; i++)
                    {
                        if (DAOFactory.MapNpcDAO.LoadById(mapnpcid) != null)
                        {
                            recipe = new RecipeDTO
                            {
                                ItemVNum = short.Parse(currentPacket[i]),
                                MapNpcId = mapnpcid
                            };
                            if (DAOFactory.RecipeDAO.LoadByNpc(mapnpcid).Any(s => s.ItemVNum == recipe.ItemVNum))
                            {
                                continue;
                            }
                            DAOFactory.RecipeDAO.Insert(recipe);
                            count++;
                        }
                    }
                    continue;
                }
                if (currentPacket.Length > 2 && currentPacket[0] == "pdtse")
                {
                    item = short.Parse(currentPacket[2]);
                    continue;
                }
                if (currentPacket.Length > 1 && currentPacket[0] == "m_list" && (currentPacket[1] == "3" || currentPacket[1] == "5"))
                {
                    for (int i = 3; i < currentPacket.Length - 1; i += 2)
                    {
                        RecipeDTO rec = DAOFactory.RecipeDAO.LoadByNpc(mapnpcid).FirstOrDefault(s => s.ItemVNum == item);
                        if (rec != null)
                        {
                            rec.Amount = byte.Parse(currentPacket[2]);
                            DAOFactory.RecipeDAO.Update(rec);
                            RecipeDTO recipedto = DAOFactory.RecipeDAO.LoadByNpc(mapnpcid).FirstOrDefault(s => s.ItemVNum == item);
                            if (recipedto != null)
                            {
                                short recipeId = recipedto.RecipeId;

                                RecipeItemDTO recipeitem = new RecipeItemDTO
                                {
                                    ItemVNum = short.Parse(currentPacket[i]),
                                    Amount = byte.Parse(currentPacket[i + 1]),
                                    RecipeId = recipeId
                                };

                                if (!DAOFactory.RecipeItemDAO.LoadByRecipeAndItem(recipeId, recipeitem.ItemVNum).Any())
                                {
                                    DAOFactory.RecipeItemDAO.Insert(recipeitem);
                                }
                            }
                        }
                    }
                    item = -1;
                }
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("RECIPES_PARSED"), count));
        }

        //Need fix
        public void ImportRespawnMapType()
        {
            List<RespawnMapTypeDTO> respawnmaptypemaps = new List<RespawnMapTypeDTO>
            {
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                    DefaultMapId = 1,
                    DefaultX = 80,
                    DefaultY = 116,
                    Name = "Default"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.ReturnAct1,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "Return"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct5,
                    DefaultMapId = 170,
                    DefaultX = 86,
                    DefaultY = 48,
                    Name = "DefaultAct5"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.ReturnAct5,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "ReturnAct5"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct6"
                }
            };
            DAOFactory.RespawnMapTypeDAO.Insert(respawnmaptypemaps);
            Logger.Log.Info(Language.Instance.GetMessageFromKey("RESPAWNTYPE_PARSED"));
        }

        public void ImportShopItems()
        {
            List<ShopItemDTO> shopitems = new List<ShopItemDTO>();
            int itemCounter = 0;
            byte type = 0;
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("n_inv") || o[0].Equals("shopping")))
            {
                if (currentPacket[0].Equals("n_inv"))
                {
                    if (DAOFactory.ShopDAO.LoadByNpc(short.Parse(currentPacket[2])) != null)
                    {
                        for (int i = 5; i < currentPacket.Length; i++)
                        {
                            string[] item = currentPacket[i].Split('.');
                            ShopItemDTO sitem = null;

                            if (item.Length == 5)
                            {
                                sitem = new ShopItemDTO
                                {
                                    ShopId = DAOFactory.ShopDAO.LoadByNpc(short.Parse(currentPacket[2])).ShopId,
                                    Type = type,
                                    Slot = byte.Parse(item[1]),
                                    ItemVNum = short.Parse(item[2])
                                };
                            }
                            else if (item.Length == 6)
                            {
                                sitem = new ShopItemDTO
                                {
                                    ShopId = DAOFactory.ShopDAO.LoadByNpc(short.Parse(currentPacket[2])).ShopId,
                                    Type = type,
                                    Slot = byte.Parse(item[1]),
                                    ItemVNum = short.Parse(item[2]),
                                    Rare = sbyte.Parse(item[3]),
                                    Upgrade = byte.Parse(item[4])
                                };
                            }

                            if (sitem == null || shopitems.Any(s => s.ItemVNum.Equals(sitem.ItemVNum) && s.ShopId.Equals(sitem.ShopId)) || DAOFactory.ShopItemDAO.LoadByShopId(sitem.ShopId).Any(s => s.ItemVNum.Equals(sitem.ItemVNum)))
                            {
                                continue;
                            }

                            shopitems.Add(sitem);
                            itemCounter++;
                        }
                    }
                }
                else
                {
                    if (currentPacket.Length > 3)
                    {
                        type = byte.Parse(currentPacket[1]);
                    }
                }
            }

            DAOFactory.ShopItemDAO.Insert(shopitems);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_PARSED"), itemCounter));
        }

        public void ImportShops()
        {
            int shopCounter = 0;
            List<ShopDTO> shops = new List<ShopDTO>();
            foreach (string[] currentPacket in _packetList.Where(o => o.Length > 6 && o[0].Equals("shop") && o[1].Equals("2")))
            {
                MapNpcDTO npc = DAOFactory.MapNpcDAO.LoadById(short.Parse(currentPacket[2]));
                if (npc == null)
                {
                    continue;
                }
                string name = string.Empty;
                for (int j = 6; j < currentPacket.Length; j++)
                {
                    name += $"{currentPacket[j]} ";
                }
                name = name.Trim();

                ShopDTO shop = new ShopDTO
                {
                    Name = name,
                    MapNpcId = npc.MapNpcId,
                    MenuType = byte.Parse(currentPacket[4]),
                    ShopType = byte.Parse(currentPacket[5])
                };

                if (DAOFactory.ShopDAO.LoadByNpc(npc.MapNpcId) == null && shops.All(s => s.MapNpcId != npc.MapNpcId))
                {
                    shops.Add(shop);
                    shopCounter++;
                }
            }

            DAOFactory.ShopDAO.Insert(shops);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPS_PARSED"), shopCounter));
        }

        public void ImportShopSkills()
        {
            List<ShopSkillDTO> shopskills = new List<ShopSkillDTO>();
            int itemCounter = 0;
            byte type = 0;
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("n_inv") || o[0].Equals("shopping")))
            {
                if (currentPacket[0].Equals("n_inv"))
                {
                    if (DAOFactory.ShopDAO.LoadByNpc(short.Parse(currentPacket[2])) != null)
                    {
                        for (int i = 5; i < currentPacket.Length; i++)
                        {
                            ShopSkillDTO sskill;
                            if (!currentPacket[i].Contains("."))
                            {
                                sskill = new ShopSkillDTO
                                {
                                    ShopId = DAOFactory.ShopDAO.LoadByNpc(short.Parse(currentPacket[2])).ShopId,
                                    Type = type,
                                    Slot = (byte)(i - 5),
                                    SkillVNum = short.Parse(currentPacket[i])
                                };

                                if (shopskills.Any(s => s.SkillVNum.Equals(sskill.SkillVNum) && s.ShopId.Equals(sskill.ShopId)) || DAOFactory.ShopSkillDAO.LoadByShopId(sskill.ShopId).Any(s => s.SkillVNum.Equals(sskill.SkillVNum)))
                                {
                                    continue;
                                }

                                shopskills.Add(sskill);
                                itemCounter++;
                            }
                        }
                    }
                }
                else
                {
                    if (currentPacket.Length > 3)
                    {
                        type = byte.Parse(currentPacket[1]);
                    }
                }
            }

            DAOFactory.ShopSkillDAO.Insert(shopskills);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPSKILLS_PARSED"), itemCounter));
        }


        public void ImportSkills()
        {
            string fileSkillId = $"{_folder}\\Skill.dat";
            string fileSkillLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_Skill.txt";
            List<SkillDTO> skills = new List<SkillDTO>();

            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            SkillDTO skill = new SkillDTO();
            List<ComboDTO> Combo = new List<ComboDTO>();
            List<BCardDTO> skillCards = new List<BCardDTO>();
            string line;
            int counter = 0;
            using (StreamReader skillIdLangStream = new StreamReader(fileSkillLang, Encoding.GetEncoding(1252)))
            {
                while ((line = skillIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }
                skillIdLangStream.Close();
            }

            using (StreamReader skillIdStream = new StreamReader(fileSkillId, Encoding.GetEncoding(1252)))
            {
                while ((line = skillIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 2 && currentLine[1] == "VNUM")
                    {
                        skill = new SkillDTO
                        {
                            SkillVNum = short.Parse(currentLine[2])
                        };
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        skill.Name = dictionaryIdLang.TryGetValue(currentLine[2], out string name) ? name : string.Empty;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "TYPE")
                    {
                        skill.SkillType = byte.Parse(currentLine[2]);
                        skill.CastId = short.Parse(currentLine[3]);
                        skill.Class = byte.Parse(currentLine[4]);
                        skill.Type = byte.Parse(currentLine[5]);
                        skill.Element = byte.Parse(currentLine[7]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "FCOMBO")
                    {
                        for (int i = 3; i < currentLine.Length - 4; i += 3)
                        {
                            ComboDTO comb = new ComboDTO
                            {
                                SkillVNum = skill.SkillVNum,
                                Hit = short.Parse(currentLine[i]),
                                Animation = short.Parse(currentLine[i + 1]),
                                Effect = short.Parse(currentLine[i + 2])
                            };

                            if (comb.Hit != 0 || comb.Animation != 0 || comb.Effect != 0)
                            {
                                if (!DAOFactory.ComboDAO.LoadByVNumHitAndEffect(comb.SkillVNum, comb.Hit, comb.Effect).Any())
                                {
                                    Combo.Add(comb);
                                }
                            }
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "COST")
                    {
                        skill.CPCost = currentLine[2] == "-1" ? (byte)0 : byte.Parse(currentLine[2]);
                        skill.Price = int.Parse(currentLine[3]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "LEVEL")
                    {
                        skill.LevelMinimum = currentLine[2] != "-1" ? byte.Parse(currentLine[2]) : (byte)0;
                        if (skill.Class > 31)
                        {
                            SkillDTO firstskill = skills.FirstOrDefault(s => s.Class == skill.Class);
                            if (firstskill == null || skill.SkillVNum <= firstskill.SkillVNum + 10)
                            {
                                switch (skill.Class)
                                {
                                    case 8:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 3:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 2:
                                                skill.LevelMinimum = 10;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }
                                        break;

                                    case 9:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 9:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 8:
                                                skill.LevelMinimum = 16;
                                                break;

                                            case 7:
                                                skill.LevelMinimum = 12;
                                                break;

                                            case 6:
                                                skill.LevelMinimum = 8;
                                                break;

                                            case 5:
                                                skill.LevelMinimum = 4;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }
                                        break;

                                    case 16:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 6:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 5:
                                                skill.LevelMinimum = 15;
                                                break;

                                            case 4:
                                                skill.LevelMinimum = 10;
                                                break;

                                            case 3:
                                                skill.LevelMinimum = 5;
                                                break;

                                            case 2:
                                                skill.LevelMinimum = 3;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }
                                        break;

                                    default:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 10:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 9:
                                                skill.LevelMinimum = 16;
                                                break;

                                            case 8:
                                                skill.LevelMinimum = 12;
                                                break;

                                            case 7:
                                                skill.LevelMinimum = 8;
                                                break;

                                            case 6:
                                                skill.LevelMinimum = 4;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                        skill.MinimumAdventurerLevel = currentLine[3] != "-1" ? byte.Parse(currentLine[3]) : (byte)0;
                        skill.MinimumSwordmanLevel = currentLine[4] != "-1" ? byte.Parse(currentLine[4]) : (byte)0;
                        skill.MinimumArcherLevel = currentLine[5] != "-1" ? byte.Parse(currentLine[5]) : (byte)0;
                        skill.MinimumMagicianLevel = currentLine[6] != "-1" ? byte.Parse(currentLine[6]) : (byte)0;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "EFFECT")
                    {
                        skill.CastEffect = short.Parse(currentLine[3]);
                        skill.CastAnimation = short.Parse(currentLine[4]);
                        skill.Effect = short.Parse(currentLine[5]);
                        skill.AttackAnimation = short.Parse(currentLine[6]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "TARGET")
                    {
                        skill.TargetType = byte.Parse(currentLine[2]);
                        skill.HitType = byte.Parse(currentLine[3]);
                        skill.Range = byte.Parse(currentLine[4]);
                        skill.TargetRange = byte.Parse(currentLine[5]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "DATA")
                    {
                        skill.UpgradeSkill = short.Parse(currentLine[2]);
                        skill.UpgradeType = short.Parse(currentLine[3]);
                        skill.CastTime = short.Parse(currentLine[6]);
                        skill.Cooldown = short.Parse(currentLine[7]);
                        skill.MpCost = short.Parse(currentLine[10]);
                        skill.ItemVNum = short.Parse(currentLine[12]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "BASIC")
                    {
                        int type = Int32.Parse(currentLine[3]);
                        if (type != 0 && type!=-1)
                        {
                            BCardDTO itemCard = new BCardDTO
                            {
                                SkillVNum = skill.SkillVNum,
                                Type =(byte)type,
                                SubType = (byte)((int.Parse(currentLine[4]) + 1) * 10),
                                FirstData = (short)(int.Parse(currentLine[5]) / 4),
                                SecondData = (short)(int.Parse(currentLine[6]) / 4),
                                ThirdData = (short)(int.Parse(currentLine[7]) / 4),
                            };
                            skillCards.Add(itemCard);
                        }
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "FCOMBO")
                    {
                        // investigate
                        /*
                        if (currentLine[2] == "1")
                        {
                            combo.FirstActivationHit = byte.Parse(currentLine[3]);
                            combo.FirstComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.FirstComboEffect = short.Parse(currentLine[5]);
                            combo.SecondActivationHit = byte.Parse(currentLine[3]);
                            combo.SecondComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.SecondComboEffect = short.Parse(currentLine[5]);
                            combo.ThirdActivationHit = byte.Parse(currentLine[3]);
                            combo.ThirdComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.ThirdComboEffect = short.Parse(currentLine[5]);
                            combo.FourthActivationHit = byte.Parse(currentLine[3]);
                            combo.FourthComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.FourthComboEffect = short.Parse(currentLine[5]);
                            combo.FifthActivationHit = byte.Parse(currentLine[3]);
                            combo.FifthComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.FifthComboEffect = short.Parse(currentLine[5]);
                        }
                        */
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "CELL")
                    {
                        // investigate
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "Z_DESC")
                    {
                        // investigate
                        if (DAOFactory.SkillDAO.LoadById(skill.SkillVNum) == null)
                        {
                            skills.Add(skill);
                            counter++;
                        }
                    }
                }
                DAOFactory.SkillDAO.Insert(skills);
                DAOFactory.ComboDAO.Insert(Combo);
                DAOFactory.BCardDAO.Insert(skillCards);

                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SKILLS_PARSED"), counter));
                skillIdStream.Close();
            }
        }

        public void ImportTeleporters()
        {
            int teleporterCounter = 0;
            TeleporterDTO teleporter = null;
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("at") || o[0].Equals("n_run") && (o[1].Equals("16") || o[1].Equals("26") || o[1].Equals("45") || o[1].Equals("301") || o[1].Equals("132") || o[1].Equals("5002") || o[1].Equals("5012"))))
            {
                if (currentPacket.Length > 4 && currentPacket[0] == "n_run")
                {
                    if (DAOFactory.MapNpcDAO.LoadById(int.Parse(currentPacket[4])) == null)
                    {
                        continue;
                    }
                    teleporter = new TeleporterDTO
                    {
                        MapNpcId = int.Parse(currentPacket[4]),
                        Index = short.Parse(currentPacket[2])
                    };
                    continue;
                }
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    if (teleporter == null)
                    {
                        continue;
                    }
                    teleporter.MapId = short.Parse(currentPacket[2]);
                    teleporter.MapX = short.Parse(currentPacket[3]);
                    teleporter.MapY = short.Parse(currentPacket[4]);

                    if (DAOFactory.TeleporterDAO.LoadFromNpc(teleporter.MapNpcId).Any(s => s.Index == teleporter.Index))
                    {
                        continue;
                    }
                    DAOFactory.TeleporterDAO.Insert(teleporter);
                    teleporterCounter++;
                    teleporter = null;
                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TELEPORTERS_PARSED"), teleporterCounter));
        }

        public void ImportScriptedInstances()
        {
            short map = 0;
            List<ScriptedInstanceDTO> listtimespace = new List<ScriptedInstanceDTO>();
            List<ScriptedInstanceDTO> bddlist = new List<ScriptedInstanceDTO>(); ;
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("at") || o[0].Equals("wp") || o[0].Equals("gp") || o[0].Equals("rbr")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    bddlist = DAOFactory.ScriptedInstanceDAO.LoadByMap(map).ToList();
                    continue;
                }
                else if (currentPacket.Length > 6 && currentPacket[0] == "wp")
                {
                    ScriptedInstanceDTO ts = new ScriptedInstanceDTO()
                    {
                        PositionX = short.Parse(currentPacket[1]),
                        PositionY = short.Parse(currentPacket[2]),
                        MapId = map,
                    };

                    if (!bddlist.Concat(listtimespace).Any(s => s.MapId == ts.MapId && s.PositionX == ts.PositionX && s.PositionY == ts.PositionY))
                    {
                        listtimespace.Add(ts);
                    }
                }
                else if (currentPacket[0] == "gp")
                {
                    if (sbyte.Parse(currentPacket[4])==(byte)PortalType.Raid)
                    {
                        ScriptedInstanceDTO ts = new ScriptedInstanceDTO()
                        {
                            PositionX = short.Parse(currentPacket[1]),
                            PositionY = short.Parse(currentPacket[2]),
                            MapId = map,
                            Type = ScriptedInstanceType.Raid,
                        };

                        if (!bddlist.Concat(listtimespace).Any(s => s.MapId == ts.MapId && s.PositionX == ts.PositionX && s.PositionY == ts.PositionY))
                        {
                            listtimespace.Add(ts);
                        }
                    }

                }
                else if (currentPacket[0] == "rbr")
                {
                    //someinfo
                }
            }
            DAOFactory.ScriptedInstanceDAO.Insert(listtimespace);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TIMESPACES_PARSED"), listtimespace.Count));
        }

        public void LoadMaps()
        {
            _maps = DAOFactory.MapDAO.LoadAll().ToList();
        }

        internal void ImportItems()
        {
            string fileId = $"{_folder}\\Item.dat";
            string fileLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_Item.txt";
            Dictionary<string, string> dictionaryName = new Dictionary<string, string>();
            string line;
            List<ItemDTO> items = new List<ItemDTO>();
            List<BCardDTO> itemCards = new List<BCardDTO>();
            using (StreamReader mapIdLangStream = new StreamReader(fileLang, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length <= 1 || dictionaryName.ContainsKey(linesave[0]))
                    {
                        continue;
                    }
                    dictionaryName.Add(linesave[0], linesave[1]);
                }
                mapIdLangStream.Close();
            }

            using (StreamReader npcIdStream = new StreamReader(fileId, Encoding.GetEncoding(1252)))
            {
                ItemDTO item = new ItemDTO();
                bool itemAreaBegin = false;
                int itemCounter = 0;

                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 3 && currentLine[1] == "VNUM")
                    {
                        itemAreaBegin = true;
                        item.VNum = short.Parse(currentLine[2]);
                        item.Price = long.Parse(currentLine[3]);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "END")
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }
                        if (DAOFactory.ItemDAO.LoadById(item.VNum) == null)
                        {
                            items.Add(item);
                            itemCounter++;
                        }
                        item = new ItemDTO();
                        itemAreaBegin = false;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        item.Name = dictionaryName.TryGetValue(currentLine[2], out string name) ? name : string.Empty;
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "INDEX")
                    {
                        switch (Convert.ToByte(currentLine[2]))
                        {
                            case 4:
                                item.Type = InventoryType.Equipment;
                                break;

                            case 8:
                                item.Type = InventoryType.Equipment;
                                break;

                            case 9:
                                item.Type = InventoryType.Main;
                                break;

                            case 10:
                                item.Type = InventoryType.Etc;
                                break;

                            default:
                                item.Type = (InventoryType)Enum.Parse(typeof(InventoryType), currentLine[2]);
                                break;
                        }
                        item.ItemType = currentLine[3] != "-1" ? (ItemType)Enum.Parse(typeof(ItemType), $"{(short)item.Type}{currentLine[3]}") : ItemType.Weapon;
                        item.ItemSubType = Convert.ToByte(currentLine[4]);
                        item.EquipmentSlot = (EquipmentType)Enum.Parse(typeof(EquipmentType), currentLine[5] != "-1" ? currentLine[5] : "0");

                        // item.DesignId = Convert.ToInt16(currentLine[6]);
                        switch (item.VNum)
                        {
                            case 1906:
                                item.Morph = 2368;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 1907:
                                item.Morph = 2370;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 1965:
                                item.Morph = 2406;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5008:
                                item.Morph = 2411;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5117:
                                item.Morph = 2429;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5152:
                                item.Morph = 2432;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5173:
                                item.Morph = 2511;
                                item.Speed = 16;
                                item.WaitDelay = 3000;
                                break;

                            case 5196:
                                item.Morph = 2517;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5226: // Invisible locomotion, only 5 seconds with booster
                                item.Morph = 1817;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5228: // Invisible locoomotion, only 5 seconds with booster
                                item.Morph = 1819;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5232:
                                item.Morph = 2520;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5234:
                                item.Morph = 2522;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5236:
                                item.Morph = 2524;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5238:
                                item.Morph = 1817;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5240:
                                item.Morph = 1819;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5319:
                                item.Morph = 2526;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5321:
                                item.Morph = 2528;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5323:
                                item.Morph = 2530;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5330:
                                item.Morph = 2928;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5332:
                                item.Morph = 2930;
                                item.Speed = 14;
                                item.WaitDelay = 3000;
                                break;

                            case 5360:
                                item.Morph = 2932;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5386:
                                item.Morph = 2934;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5387:
                                item.Morph = 2936;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5388:
                                item.Morph = 2938;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5389:
                                item.Morph = 2940;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5390:
                                item.Morph = 2942;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5391:
                                item.Morph = 2944;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5914:
                                item.Morph = 2513;
                                item.Speed = 14;
                                item.WaitDelay = 3000;
                                break;

                            case 9054:
                                item.Morph = 2368;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9055:
                                item.Morph = 2370;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9058:
                                item.Morph = 2406;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9065:
                                item.Morph = 2411;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9070:
                                item.Morph = 2429;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9073:
                                item.Morph = 2432;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9078:
                                item.Morph = 2520;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9079:
                                item.Morph = 2522;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9080:
                                item.Morph = 2524;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9081:
                                item.Morph = 1817;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9082:
                                item.Morph = 1819;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9083:
                                item.Morph = 2526;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9084:
                                item.Morph = 2528;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9085:
                                item.Morph = 2930;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9086:
                                item.Morph = 2928;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9087:
                                item.Morph = 2930;
                                item.Speed = 14;
                                item.WaitDelay = 3000;
                                break;

                            case 9088:
                                item.Morph = 2932;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9090:
                                item.Morph = 2934;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9091:
                                item.Morph = 2936;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9092:
                                item.Morph = 2938;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9093:
                                item.Morph = 2940;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9094:
                                item.Morph = 2942;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            default:
                                if (item.EquipmentSlot.Equals(EquipmentType.Amulet))
                                {
                                    switch (item.VNum)
                                    {
                                        case 4503:
                                            item.EffectValue = 4544;
                                            break;

                                        case 4504:
                                            item.EffectValue = 4294;
                                            break;

                                        default:
                                            item.EffectValue = Convert.ToInt16(currentLine[7]);
                                            break;
                                    }
                                }
                                else
                                {
                                    item.Morph = Convert.ToInt16(currentLine[7]);
                                }
                                break;
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "TYPE")
                    {
                        // currentLine[2] 0-range 2-range 3-magic
                        item.Class = item.EquipmentSlot == EquipmentType.Fairy ? (byte)15 : Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "FLAG")
                    {
                        item.IsSoldable = currentLine[5] == "0";
                        item.IsDroppable = currentLine[6] == "0";
                        item.IsTradable = currentLine[7] == "0";
                        item.IsBlocked = currentLine[8] == "1";
                        item.IsMinilandObject = currentLine[9] == "1";
                        item.IsHolder = currentLine[10] == "1";
                        item.IsColored = currentLine[16] == "1";
                        item.Sex = currentLine[18] == "1" ? (byte)1 : currentLine[17] == "1" ? (byte)2 : (byte)0;
                        if (currentLine[21] == "1")
                        {
                            item.ReputPrice = item.Price;
                        }
                        item.IsHeroic = currentLine[22] == "1";
                        /*
                        item.IsVehicle = currentLine[11] == "1" ? true : false; // (?)
                        item.BoxedVehicle = currentLine[12] == "1" ? true : false; // (?)
                        linesave[4]  unknown
                        linesave[11] unknown
                        linesave[12] unknown
                        linesave[13] unknown
                        linesave[14] unknown
                        linesave[15] unknown
                        linesave[19] unknown
                        linesave[20] unknown
                        */
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "DATA")
                    {
                        switch (item.ItemType)
                        {
                            case ItemType.Weapon:
                                item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                item.DamageMinimum = Convert.ToInt16(currentLine[3]);
                                item.DamageMaximum = Convert.ToInt16(currentLine[4]);
                                item.HitRate = Convert.ToInt16(currentLine[5]);
                                item.CriticalLuckRate = Convert.ToByte(currentLine[6]);
                                item.CriticalRate = Convert.ToInt16(currentLine[7]);
                                item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                                item.MaximumAmmo = 100;
                                break;

                            case ItemType.Armor:
                                item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                item.CloseDefence = Convert.ToInt16(currentLine[3]);
                                item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                                item.MagicDefence = Convert.ToInt16(currentLine[5]);
                                item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                                item.DistanceDefenceDodge = Convert.ToInt16(currentLine[6]);
                                item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                                break;

                            case ItemType.Box:
                                switch (item.VNum)
                                {
                                    // add here your custom effect/effectvalue for box item, make
                                    // sure its unique for boxitems

                                    case 287:
                                        item.Effect = 69;
                                        item.EffectValue = 1;
                                        break;

                                    case 4240:
                                        item.Effect = 69;
                                        item.EffectValue = 2;
                                        break;

                                    case 4194:
                                        item.Effect = 69;
                                        item.EffectValue = 3;
                                        break;

                                    case 4106:
                                        item.Effect = 69;
                                        item.EffectValue = 4;
                                        break;

                                    default:
                                        item.Effect = Convert.ToInt16(currentLine[2]);
                                        item.EffectValue = Convert.ToInt32(currentLine[3]);
                                        item.LevelMinimum = Convert.ToByte(currentLine[4]);
                                        break;
                                }
                                break;

                            case ItemType.Fashion:
                                item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                item.CloseDefence = Convert.ToInt16(currentLine[3]);
                                item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                                item.MagicDefence = Convert.ToInt16(currentLine[5]);
                                item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                                if (item.EquipmentSlot.Equals(EquipmentType.CostumeHat) || item.EquipmentSlot.Equals(EquipmentType.CostumeSuit))
                                {
                                    item.ItemValidTime = Convert.ToInt32(currentLine[13]) * 3600;
                                }
                                break;

                            case ItemType.Food:
                                item.Hp = Convert.ToInt16(currentLine[2]);
                                item.Mp = Convert.ToInt16(currentLine[4]);
                                break;

                            case ItemType.Jewelery:
                                if (item.EquipmentSlot.Equals(EquipmentType.Amulet))
                                {
                                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                    if (item.VNum > 4055 && item.VNum < 4061 || item.VNum > 4172 && item.VNum < 4176)
                                    {
                                        item.ItemValidTime = 10800;
                                    }
                                    else if (item.VNum > 4045 && item.VNum < 4056 || item.VNum == 967 || item.VNum == 968)
                                    {
                                        // (item.VNum > 8104 && item.VNum < 8115) <= disaled for now
                                        // because doesn't work!
                                        item.ItemValidTime = 3600;
                                    }
                                    else
                                    {
                                        item.ItemValidTime = Convert.ToInt32(currentLine[3]) / 10;
                                    }
                                }
                                else if (item.EquipmentSlot.Equals(EquipmentType.Fairy))
                                {
                                    item.Element = Convert.ToByte(currentLine[2]);
                                    item.ElementRate = Convert.ToInt16(currentLine[3]);
                                    if (item.VNum <= 256)
                                    {
                                        item.MaxElementRate = 50;
                                    }
                                    else
                                    {
                                        if (item.ElementRate == 0)
                                        {
                                            if (item.VNum >= 800 && item.VNum <= 804)
                                            {
                                                item.MaxElementRate = 50;
                                            }
                                            else
                                            {
                                                item.MaxElementRate = 70;
                                            }
                                        }
                                        else if (item.ElementRate == 30)
                                        {
                                            if (item.VNum >= 884 && item.VNum <= 887)
                                            {
                                                item.MaxElementRate = 50;
                                            }
                                            else
                                            {
                                                item.MaxElementRate = 30;
                                            }
                                        }
                                        else if (item.ElementRate == 35)
                                        {
                                            item.MaxElementRate = 35;
                                        }
                                        else if (item.ElementRate == 40)
                                        {
                                            item.MaxElementRate = 70;
                                        }
                                        else if (item.ElementRate == 50)
                                        {
                                            item.MaxElementRate = 80;
                                        }
                                    }
                                }
                                else
                                {
                                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                    item.MaxCellonLvl = Convert.ToByte(currentLine[3]);
                                    item.MaxCellon = Convert.ToByte(currentLine[4]);
                                }
                                break;

                            case ItemType.Event:
                                switch (item.VNum)
                                {
                                    case 1332:
                                        item.EffectValue = 5108;
                                        break;

                                    case 1333:
                                        item.EffectValue = 5109;
                                        break;

                                    case 1334:
                                        item.EffectValue = 5111;
                                        break;

                                    case 1335:
                                        item.EffectValue = 5107;
                                        break;

                                    case 1336:
                                        item.EffectValue = 5106;
                                        break;

                                    case 1337:
                                        item.EffectValue = 5110;
                                        break;

                                    case 1339:
                                        item.EffectValue = 5114;
                                        break;

                                    case 9031:
                                        item.EffectValue = 5108;
                                        break;

                                    case 9032:
                                        item.EffectValue = 5109;
                                        break;

                                    case 9033:
                                        item.EffectValue = 5011;
                                        break;

                                    case 9034:
                                        item.EffectValue = 5107;
                                        break;

                                    case 9035:
                                        item.EffectValue = 5106;
                                        break;

                                    case 9036:
                                        item.EffectValue = 5110;
                                        break;

                                    case 9038:
                                        item.EffectValue = 5114;
                                        break;

                                    // EffectItems aka. fireworks
                                    case 1581:
                                        item.EffectValue = 860;
                                        break;

                                    case 1582:
                                        item.EffectValue = 861;
                                        break;

                                    case 1585:
                                        item.EffectValue = 859;
                                        break;

                                    case 1983:
                                        item.EffectValue = 875;
                                        break;

                                    case 1984:
                                        item.EffectValue = 876;
                                        break;

                                    case 1985:
                                        item.EffectValue = 877;
                                        break;

                                    case 1986:
                                        item.EffectValue = 878;
                                        break;

                                    case 1987:
                                        item.EffectValue = 879;
                                        break;

                                    case 1988:
                                        item.EffectValue = 880;
                                        break;

                                    case 9044:
                                        item.EffectValue = 859;
                                        break;

                                    case 9059:
                                        item.EffectValue = 875;
                                        break;

                                    case 9060:
                                        item.EffectValue = 876;
                                        break;

                                    case 9061:
                                        item.EffectValue = 877;
                                        break;

                                    case 9062:
                                        item.EffectValue = 878;
                                        break;

                                    case 9063:
                                        item.EffectValue = 879;
                                        break;

                                    case 9064:
                                        item.EffectValue = 880;
                                        break;

                                    default:
                                        item.EffectValue = Convert.ToInt16(currentLine[7]);
                                        break;
                                }
                                break;

                            case ItemType.Special:
                                switch (item.VNum)
                                {
                                    case 1246:
                                    case 9020:
                                        item.Effect = 6600;
                                        item.EffectValue = 1;
                                        break;

                                    case 1247:
                                    case 9021:
                                        item.Effect = 6600;
                                        item.EffectValue = 2;
                                        break;

                                    case 1248:
                                    case 9022:
                                        item.Effect = 6600;
                                        item.EffectValue = 3;
                                        break;

                                    case 1249:
                                    case 9023:
                                        item.Effect = 6600;
                                        item.EffectValue = 4;
                                        break;

                                    case 5130:
                                    case 9072:
                                        item.Effect = 1006;
                                        break;

                                    case 1272:
                                    case 1858:
                                    case 9047:
                                        item.Effect = 1005;
                                        item.EffectValue = 10;
                                        break;

                                    case 1273:
                                    case 9024:
                                        item.Effect = 1005;
                                        item.EffectValue = 30;
                                        break;

                                    case 1274:
                                    case 9025:
                                        item.Effect = 1005;
                                        item.EffectValue = 60;
                                        break;

                                    case 1279:
                                    case 9029:
                                        item.Effect = 1007;
                                        item.EffectValue = 30;
                                        break;

                                    case 1280:
                                    case 9030:
                                        item.Effect = 1007;
                                        item.EffectValue = 60;
                                        break;

                                    case 1923:
                                    case 9056:
                                        item.Effect = 1007;
                                        item.EffectValue = 10;
                                        break;

                                    case 1275:
                                    case 1886:
                                    case 9026:
                                        item.Effect = 1008;
                                        item.EffectValue = 10;
                                        break;

                                    case 1276:
                                    case 9027:
                                        item.Effect = 1008;
                                        item.EffectValue = 30;
                                        break;

                                    case 1277:
                                    case 9028:
                                        item.Effect = 1008;
                                        item.EffectValue = 60;
                                        break;

                                    case 5060:
                                    case 9066:
                                        item.Effect = 1003;
                                        item.EffectValue = 30;
                                        break;

                                    case 5061:
                                    case 9067:
                                        item.Effect = 1004;
                                        item.EffectValue = 7;
                                        break;

                                    case 5062:
                                    case 9068:
                                        item.Effect = 1004;
                                        item.EffectValue = 1;
                                        break;

                                    case 5105:
                                        item.Effect = 651;
                                        break;

                                    case 5115:
                                        item.Effect = 652;
                                        break;

                                    case 1981:
                                        item.Effect = 34; // imagined number as for I = √(-1), complex z = a + bi
                                        break;

                                    case 1982:
                                        item.Effect = 6969; // imagined number as for I = √(-1), complex z = a + bi
                                        break;

                                    default:
                                        if (item.VNum > 5891 && item.VNum < 5900 || item.VNum > 9100 && item.VNum < 9109)
                                        {
                                            item.Effect = 69; // imagined number as for I = √(-1), complex z = a + bi
                                        }
                                        else
                                        {
                                            item.Effect = Convert.ToInt16(currentLine[2]);
                                        }
                                        break;
                                }
                                switch (item.Effect)
                                {
                                    case 150:
                                    case 151:
                                        if (Convert.ToInt32(currentLine[4]) == 1)
                                        {
                                            item.EffectValue = 30000;
                                        }
                                        else if (Convert.ToInt32(currentLine[4]) == 2)
                                        {
                                            item.EffectValue = 70000;
                                        }
                                        else if (Convert.ToInt32(currentLine[4]) == 3)
                                        {
                                            item.EffectValue = 180000;
                                        }
                                        else
                                        {
                                            item.EffectValue = Convert.ToInt32(currentLine[4]);
                                        }
                                        break;

                                    case 204:
                                        item.EffectValue = 10000;
                                        break;

                                    case 305:
                                        item.EffectValue = Convert.ToInt32(currentLine[5]);
                                        item.Morph = Convert.ToInt16(currentLine[4]);
                                        break;

                                    default:
                                        item.EffectValue = item.EffectValue == 0 ? Convert.ToInt32(currentLine[4]) : item.EffectValue;
                                        break;
                                }
                                item.WaitDelay = 5000;
                                break;

                            case ItemType.Magical:
                                if (item.VNum > 2059 && item.VNum < 2070)
                                {
                                    item.Effect = 10;
                                }
                                else
                                {
                                    item.Effect = Convert.ToInt16(currentLine[2]);
                                }
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Specialist:

                                // item.isSpecialist = Convert.ToByte(currentLine[2]); item.Unknown = Convert.ToInt16(currentLine[3]);
                                item.ElementRate = Convert.ToInt16(currentLine[4]);
                                item.Speed = Convert.ToByte(currentLine[5]);
                                item.SpType = Convert.ToByte(currentLine[13]);

                                // item.Morph = Convert.ToInt16(currentLine[14]) + 1;
                                item.FireResistance = Convert.ToByte(currentLine[15]);
                                item.WaterResistance = Convert.ToByte(currentLine[16]);
                                item.LightResistance = Convert.ToByte(currentLine[17]);
                                item.DarkResistance = Convert.ToByte(currentLine[18]);

                                // item.PartnerClass = Convert.ToInt16(currentLine[19]);
                                item.LevelJobMinimum = Convert.ToByte(currentLine[20]);
                                item.ReputationMinimum = Convert.ToByte(currentLine[21]);

                                Dictionary<int, int> elementdic = new Dictionary<int, int> { { 0, 0 } };
                                if (item.FireResistance != 0)
                                {
                                    elementdic.Add(1, item.FireResistance);
                                }
                                if (item.WaterResistance != 0)
                                {
                                    elementdic.Add(2, item.WaterResistance);
                                }
                                if (item.LightResistance != 0)
                                {
                                    elementdic.Add(3, item.LightResistance);
                                }
                                if (item.DarkResistance != 0)
                                {
                                    elementdic.Add(4, item.DarkResistance);
                                }

                                item.Element = (byte)elementdic.OrderByDescending(s => s.Value).First().Key;
                                if (elementdic.Count > 1 && elementdic.OrderByDescending(s => s.Value).First().Value == elementdic.OrderByDescending(s => s.Value).ElementAt(1).Value)
                                {
                                    item.SecondaryElement = (byte)elementdic.OrderByDescending(s => s.Value).ElementAt(1).Key;
                                }

                                // needs to be hardcoded
                                switch (item.VNum)
                                {
                                    case 901:
                                        item.Element = 1;
                                        break;

                                    case 903:
                                        item.Element = 2;
                                        break;

                                    case 906:
                                        item.Element = 3;
                                        break;

                                    case 909:
                                        item.Element = 3;
                                        break;
                                }
                                break;

                            case ItemType.Shell:

                                // item.ShellMinimumLevel = Convert.ToInt16(linesave[3]);
                                // item.ShellMaximumLevel = Convert.ToInt16(linesave[4]);
                                // item.ShellType = Convert.ToByte(linesave[5]); // 3 shells of each type
                                break;

                            case ItemType.Main:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Upgrade:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                switch (item.VNum)
                                {
                                    // UpgradeItems (needed to be hardcoded)
                                    case 1218:
                                        item.EffectValue = 26;
                                        break;

                                    case 1363:
                                        item.EffectValue = 27;
                                        break;

                                    case 1364:
                                        item.EffectValue = 28;
                                        break;

                                    case 5107:
                                        item.EffectValue = 47;
                                        break;

                                    case 5207:
                                        item.EffectValue = 50;
                                        break;

                                    case 5369:
                                        item.EffectValue = 61;
                                        break;

                                    case 5519:
                                        item.EffectValue = 60;
                                        break;

                                    default:
                                        item.EffectValue = Convert.ToInt32(currentLine[4]);
                                        break;
                                }
                                break;

                            case ItemType.Production:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Map:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Potion:
                                item.Hp = Convert.ToInt16(currentLine[2]);
                                item.Mp = Convert.ToInt16(currentLine[4]);
                                break;

                            case ItemType.Snack:
                                item.Hp = Convert.ToInt16(currentLine[2]);
                                item.Mp = Convert.ToInt16(currentLine[4]);
                                break;

                            case ItemType.Teacher:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);

                                // item.PetLoyality = Convert.ToInt16(linesave[4]); item.PetFood = Convert.ToInt16(linesave[7]);
                                break;

                            case ItemType.Part:

                                // nothing to parse
                                break;

                            case ItemType.Sell:

                                // nothing to parse
                                break;

                            case ItemType.Quest2:

                                // nothing to parse
                                break;

                            case ItemType.Quest1:

                                // nothing to parse
                                break;

                            case ItemType.Ammo:

                                // nothing to parse
                                break;
                        }

                        if (item.Type == InventoryType.Miniland)
                        {
                            item.MinilandObjectPoint = int.Parse(currentLine[2]);
                            item.EffectValue = short.Parse(currentLine[8]);
                            item.Width = Convert.ToByte(currentLine[9]);
                            item.Height = Convert.ToByte(currentLine[10]);
                        }

                        if ((item.EquipmentSlot == EquipmentType.Boots || item.EquipmentSlot == EquipmentType.Gloves) && item.Type == 0)
                        {
                            item.FireResistance = Convert.ToByte(currentLine[7]);
                            item.WaterResistance = Convert.ToByte(currentLine[8]);
                            item.LightResistance = Convert.ToByte(currentLine[9]);
                            item.DarkResistance = Convert.ToByte(currentLine[11]);
                        }
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "BUFF")
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int type = Int32.Parse(currentLine[2 + 5 * i]);
                            if (type != 0 && type != -1)
                            {
                                BCardDTO itemCard = new BCardDTO
                                {
                                    ItemVNum = item.VNum,
                                    Type = (byte)type,
                                    SubType = (byte)((int.Parse(currentLine[5 + 5 * i]) + 1)),
                                    FirstData = (short)(int.Parse(currentLine[3 + 5 * i]) / 4),
                                    SecondData = (short)(int.Parse(currentLine[4 + 5 * i]) / 4),
                                    ThirdData = (short)(int.Parse(currentLine[6 + 5 * i]) / 4),
                                };
                                itemCards.Add(itemCard);
                            }

                        }
                    }
                }

                DAOFactory.ItemDAO.Insert(items);
                DAOFactory.BCardDAO.Insert(itemCards);
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ITEMS_PARSED"), itemCounter));
                npcIdStream.Close();
            }
        }

        #endregion
    }
}