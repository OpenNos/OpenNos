using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Import.Console
{
    public class ImportFactory
    {
        private string _folder;

        public ImportFactory(string folder)
        {
            _folder = folder;
        }

        public void ImportItems()
        {
            string file = $"{_folder}\\Item.dat";
            IEnumerable<ItemDTO> items = DatParser.Parse<ItemDTO>(file);

            int i = 0;
        }

        public void ImportPortals()
        {
            string filePacket = $"{_folder}\\packet.txt";
            StreamReader Packet = new StreamReader(filePacket, Encoding.GetEncoding(1252));
            string line;
            int i = 0;
            List<PortalDTO> ListPacket = new List<PortalDTO>();
            List<PortalDTO> ListPortal = new List<PortalDTO>();
            short map = 0;
            short lastMap = 0;
            while ((line = Packet.ReadLine()) != null)
            {

                string[] linesave = line.Split(' ');
                if (linesave.Count() > 5 && linesave[0] == "at")
                {
                    lastMap = map;
                    map = short.Parse(linesave[2]);


                }
                if (linesave.Count() > 4 && linesave[0] == "gp")
                {
                    short SourceX = short.Parse(linesave[1]);
                    short Type = short.Parse(linesave[4]);
                    short SourceY = short.Parse(linesave[2]);
                    short DestinationMapId = short.Parse(linesave[3]);

                    if(ListPacket.FirstOrDefault(s=>s.SourceMapId == map && s.SourceX == SourceX && s.SourceY == SourceY && s.DestinationMapId == DestinationMapId) ==null)
                    ListPacket.Add(new PortalDTO
                    {
                        SourceMapId = map,
                        SourceX = SourceX,
                        SourceY = SourceY,
                        DestinationMapId = DestinationMapId,
                        Type = Type,
                        DestinationX = -1,
                        DestinationY = -1,
                        IsDisabled = 0,
                    });
                }

            }
            ListPacket = ListPacket.OrderBy(s => s.SourceMapId).ThenBy(s => s.DestinationMapId).ThenBy(s => s.SourceY).ThenBy(s => s.SourceX).ToList();
            foreach (PortalDTO portal in ListPacket)
            {
                PortalDTO p = ListPacket.Except(ListPortal).FirstOrDefault(s => s.SourceMapId.Equals(portal.DestinationMapId) && s.DestinationMapId.Equals(portal.SourceMapId));
                if (p != null)
                {
                    portal.DestinationX = p.SourceX;
                    portal.DestinationY = p.SourceY;
                    p.DestinationY = portal.SourceY;
                    p.DestinationX = portal.SourceX;
                    ListPortal.Add(p);
                    ListPortal.Add(portal);
                }
            }


            foreach (PortalDTO portal in ListPortal)
            {
                if (DAOFactory.PortalDAO.LoadFromMap(portal.SourceMapId).Where(s => s.DestinationMapId.Equals(portal.DestinationMapId) && s.SourceX.Equals(portal.SourceX) && s.SourceY.Equals(portal.SourceY)).Count() == 0)
                {
                    DAOFactory.PortalDAO.Insert(portal);
                    i++;
                }
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("PORTALS_PARSED"), i));
        }
        public void importShops()
        {
            string filePacket = $"{_folder}\\packet.txt";
            StreamReader Packet = new StreamReader(filePacket, Encoding.GetEncoding(1252));
            string line;
            short lastMap = 0;
            short map = 0;
            int i = 0;
            Dictionary<int, int> dictionaryId = new Dictionary<int, int>();
            while ((line = Packet.ReadLine()) != null)
            {

                string[] linesave = line.Split(' ');
                if (linesave.Count() > 5 && linesave[0] == "at")
                {
                    lastMap = map;
                    map = short.Parse(linesave[2]);
                }
                if (linesave.Count() > 7 && linesave[0] == "in" && linesave[1] == "2")
                {
                    if (long.Parse(linesave[3]) < 10000)
                    {
                        NpcDTO npc = DAOFactory.NpcDAO.LoadFromMap(map).FirstOrDefault(s => s.MapId.Equals(map) && s.Vnum.Equals(short.Parse(linesave[2])));
                        if (npc != null)
                        {
                            if (!dictionaryId.ContainsKey(short.Parse(linesave[3])))
                                dictionaryId[short.Parse(linesave[3])] = npc.NpcId;
                        }
                    }
                }
                if (linesave.Count() > 6 && linesave[0] == "shop" && linesave[1] == "2")
                {

                    if (dictionaryId.ContainsKey(short.Parse(linesave[2])))
                    {
                        string named = "";
                        for (int j = 6; j < linesave.Count(); j++)
                        {
                            named += $"{linesave[j]} ";
                        }
                        named.TrimEnd(' ');
                        ShopDTO shop = new ShopDTO
                        {
                            Name = named,
                            NpcId = (short)dictionaryId[short.Parse(linesave[2])],
                            MenuType = short.Parse(linesave[4]),
                            ShopType = short.Parse(linesave[5]),
                        };
                        if (DAOFactory.ShopDAO.LoadByNpc(shop.NpcId) == null)
                        {
                            DAOFactory.ShopDAO.Insert(shop);
                            i++;
                        }
                    }
                }
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("SHOPS_PARSED"), i));
        }
        public void ImportNpcs()
        {
            string NpcIdFile = $"{_folder}\\monster.dat";
            string filePacket = $"{_folder}\\packet.txt";
            string NpcText = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_monster.txt";
            StreamReader NpcId = new StreamReader(NpcIdFile, Encoding.GetEncoding(1252));
            StreamReader NpcIdText = new StreamReader(NpcText, Encoding.GetEncoding(1252));
            StreamReader Packet = new StreamReader(filePacket, Encoding.GetEncoding(1252));
            Dictionary<int, string> dictionaryId = new Dictionary<int, string>();
            Dictionary<int, string> name = new Dictionary<int, string>();
            Dictionary<int, int> level = new Dictionary<int, int>();
            Dictionary<int, int> dialog = new Dictionary<int, int>();
            Dictionary<string, string> dictionaryTextId = new Dictionary<string, string>();
            string line;
            string line2;
            while ((line = NpcIdText.ReadLine()) != null)
            {
                string[] linesave = line.Split('\t');
                if (linesave.Count() > 1)
                {

                    if (!dictionaryTextId.ContainsKey(linesave[0]))
                        dictionaryTextId.Add(linesave[0], linesave[1]);

                }
            }
            int vnum = -1;
            int level2 = 0;
            string name2 = "";
            bool test = false;
            while ((line2 = NpcId.ReadLine()) != null)
            {

                string[] linesave = line2.Split('\t');

                if (linesave.Count() > 2 && linesave[1] == "VNUM")
                {
                    vnum = int.Parse(linesave[2]);
                    test = true;
                }
                if (linesave.Count() > 2 && linesave[1] == "LEVEL")
                {
                    level2 = int.Parse(linesave[2]);
                    if (test == true)
                    {
                        level.Add(vnum, level2);
                        name.Add(vnum, name2);
                        test = false;
                    }
                }

                if (linesave.Count() > 2 && linesave[1] == "NAME")
                {
                    name2 = linesave[2];
                }

            }

            int i = 0;
            short map = 0;
            short lastMap = 0;


            Packet = new StreamReader(filePacket, Encoding.GetEncoding(1252));
            while ((line = Packet.ReadLine()) != null)
            {

                string[] linesave = line.Split(' ');
                if (linesave.Count() > 5 && linesave[0] == "at")
                {
                    lastMap = map;
                    map = short.Parse(linesave[2]);
                }
                if (linesave.Count() > 7 && linesave[0] == "in" && linesave[1] == "2")
                {
                    if (long.Parse(linesave[3]) < 10000)
                    {
                        int dialogn = 0;
                        if (dialog.ContainsKey(int.Parse(linesave[3])))
                            dialogn = dialog[int.Parse(linesave[3])];
                        if (DAOFactory.NpcDAO.LoadFromMap(map).FirstOrDefault(s => s.MapId.Equals(map) && s.Vnum.Equals(short.Parse(linesave[2]))) == null)
                        {
                            DAOFactory.NpcDAO.Insert(new NpcDTO
                            {
                                Vnum = short.Parse(linesave[2]),
                                Level = (short)level[int.Parse(linesave[2])],
                                MapId = map,
                                MapX = short.Parse(linesave[4]),
                                MapY = short.Parse(linesave[5]),
                                Name = dictionaryTextId[name[int.Parse(linesave[2])]],
                                Position = short.Parse(linesave[6]),
                                Dialog = short.Parse(linesave[9]),
                            });
                            i++;
                        }
                    }
                }


            }
            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCS_PARSED"), i));
        }

        public void ImportMaps()
        {

            string mapIdFile = $"{_folder}\\MapIDData.dat";
            string mapIdText = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt";
            string file = $"{_folder}\\map";
            string filePacket = $"{_folder}\\packet.txt";

            StreamReader MapId = new StreamReader(mapIdFile, Encoding.GetEncoding(1252));
            StreamReader MapIdText = new StreamReader(mapIdText, Encoding.GetEncoding(1252));
            StreamReader Packet = new StreamReader(filePacket, Encoding.GetEncoding(1252));

            Dictionary<int, string> dictionaryId = new Dictionary<int, string>();
            Dictionary<string, string> dictionaryTextId = new Dictionary<string, string>();
            Dictionary<int, int> dictionaryMusic = new Dictionary<int, int>();

            DirectoryInfo dir = new DirectoryInfo(file);
            FileInfo[] fichiers = dir.GetFiles();
            string line;
            string line2;
            int mapid;
            int i = 0;
            while ((line = MapId.ReadLine()) != null)
            {
                string[] linesave = line.Split(' ');
                if (linesave.Count() > 1)
                {
                    if (int.TryParse(linesave[0], out mapid))
                    {
                        if (!dictionaryId.ContainsKey(int.Parse(linesave[0])))
                            dictionaryId.Add(int.Parse(linesave[0]), linesave[4]);
                    }
                }
            }

            while ((line2 = MapIdText.ReadLine()) != null)
            {
                string[] linesave = line2.Split('\t');
                if (linesave.Count() > 1)
                {
                    dictionaryTextId.Add(linesave[0], linesave[1]);
                }
            }

            while ((line2 = Packet.ReadLine()) != null)
            {
                string[] linesave = line2.Split(' ');
                if (linesave.Count() > 7 && linesave[0] == "at")
                {
                    if (!dictionaryMusic.ContainsKey(int.Parse(linesave[2])))
                        dictionaryMusic.Add(int.Parse(linesave[2]), int.Parse(linesave[7]));
                }
            }


            foreach (FileInfo fichier in fichiers)
            {
                string name = "";
                int music = 0;
                if (dictionaryId.ContainsKey(int.Parse(fichier.Name)))
                    if (dictionaryTextId.ContainsKey(dictionaryId[int.Parse(fichier.Name)]))
                        name = dictionaryTextId[dictionaryId[int.Parse(fichier.Name)]];
                if (dictionaryMusic.ContainsKey(int.Parse(fichier.Name)))
                    music = dictionaryMusic[int.Parse(fichier.Name)];
                MapDTO map = new MapDTO { Name = name, Music = music, MapId = short.Parse(fichier.Name), Data = System.IO.File.ReadAllBytes(fichier.FullName) };
                if (DAOFactory.MapDAO.LoadById(map.MapId) == null)
                {
                    DAOFactory.MapDAO.Insert(map);
                    i++;
                }
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAPS_PARSED"), i));
        }
    }
}
