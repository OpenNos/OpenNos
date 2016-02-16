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
            short map = 0;
            short lastMap = 0;
            while ((line = Packet.ReadLine()) != null)
            {

                string[] linesave = line.Split(' ');
                if (linesave.Count() > 5 && linesave[0] == "at")
                {
                    lastMap = map;
                    map = short.Parse(linesave[2]);
                    if (lastMap != 0)
                    {
                        if (ListPacket.FirstOrDefault(s => s.SourceMapId == lastMap && s.DestinationMapId == map) != null)
                        {
                            ListPacket.FirstOrDefault(s => s.SourceMapId == lastMap && s.DestinationMapId == map).DestinationX = short.Parse(linesave[3]);
                            ListPacket.FirstOrDefault(s => s.SourceMapId == lastMap && s.DestinationMapId == map).DestinationY = short.Parse(linesave[4]);

                        }
                    }
                }
                if (linesave.Count() > 4 && linesave[0] == "gp")
                {
                    short SourceX = short.Parse(linesave[1]);
                    short Type = short.Parse(linesave[4]);
                    short SourceY = short.Parse(linesave[2]);
                    short DestinationMapId = short.Parse(linesave[3]);
                    short DestinationX = -1;
                    short DestinationY = -1;
                    ListPacket.Add(new PortalDTO
                    {
                        SourceMapId = map,
                        SourceX = SourceX,
                        SourceY = SourceY,
                        DestinationMapId = DestinationMapId,
                        Type = Type,
                        DestinationX = DestinationX,
                        DestinationY = DestinationY
                    });
                }
            }
            foreach (PortalDTO portal in ListPacket)
            {
                if(portal.DestinationX.Equals(-1) || portal.DestinationY.Equals(-1))
                {
                    PortalDTO test = ListPacket.FirstOrDefault(s => s.SourceMapId == portal.DestinationMapId && s.DestinationMapId == portal.SourceMapId);
                    if (test != null)
                    {
                        portal.DestinationX = test.SourceX;
                        portal.DestinationY = test.SourceY;
                    }

                }
                    PortalDTO por = new PortalDTO
                {
                    SourceMapId = portal.SourceMapId,
                    SourceX = portal.SourceX,
                    SourceY = portal.SourceY,
                    DestinationMapId = portal.DestinationMapId,
                    Type = portal.Type,
                    DestinationX = portal.DestinationX,
                    DestinationY = portal.DestinationY
                };
                if (DAOFactory.PortalDAO.LoadFromMap(portal.SourceMapId).Where(s => s.DestinationMapId.Equals(portal.DestinationMapId)).Count() == 0)
                    if (DAOFactory.MapDAO.LoadById(por.SourceMapId) != null && DAOFactory.MapDAO.LoadById(por.DestinationMapId) != null && por.DestinationX != -1 && por.DestinationY != -1)
                    {
                        DAOFactory.PortalDAO.Insert(por);
                        i++;
                    }
            }
            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("PORTALS_PARSED"), i));
        }
        public void ImportMaps()
        {

            string mapIdFile = $"{_folder}\\MapIDData.dat";
            string mapIdText = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt";
            string file = $"{_folder}\\map";
            string filePacket = $"{_folder}\\packet.txt";
            DirectoryInfo dir = new DirectoryInfo(file);
            FileInfo[] fichiers = dir.GetFiles();
            StreamReader MapId = new StreamReader(mapIdFile, Encoding.GetEncoding(1252));
            StreamReader MapIdText = new StreamReader(mapIdText, Encoding.GetEncoding(1252));
            StreamReader Packet = new StreamReader(filePacket, Encoding.GetEncoding(1252));
            Dictionary<int, string> dictionaryId = new Dictionary<int, string>();
            Dictionary<string, string> dictionaryTextId = new Dictionary<string, string>();
            Dictionary<int, int> dictionaryMusic = new Dictionary<int, int>();

            string line;
            string line2;
            int mapid;
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
            int i = 0;
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
                DAOFactory.MapDAO.Insert(map);
                i++;
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAPS_PARSED"), i));
        }
    }
}
