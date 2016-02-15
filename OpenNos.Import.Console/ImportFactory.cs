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

        public void ImportMaps()
        {
            try
            {
                string mapIdFile = $"{_folder}\\MapIDData.dat";
                string mapIdText = $"{_folder}\\_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt";
                string file = $"{_folder}\\map";
                DirectoryInfo dir = new DirectoryInfo(file);
                FileInfo[] fichiers = dir.GetFiles();
                StreamReader MapId = new StreamReader(mapIdFile, Encoding.GetEncoding(1252));
                StreamReader MapIdText = new StreamReader(mapIdText, Encoding.GetEncoding(1252));
                Dictionary<int, string> dictionaryId = new Dictionary<int, string>();
                Dictionary<string, string> dictionaryTextId = new Dictionary<string, string>();

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

                foreach (FileInfo fichier in fichiers)
                {
                    string name = "";
                    if (dictionaryId.ContainsKey(int.Parse(fichier.Name)))
                        if (dictionaryTextId.ContainsKey(dictionaryId[int.Parse(fichier.Name)]))
                            name= dictionaryTextId[dictionaryId[int.Parse(fichier.Name)]];
                    MapDTO map = new MapDTO { Name = name, Music = 0, MapId = short.Parse(fichier.Name), Data = System.IO.File.ReadAllBytes(fichier.FullName) };
                    DAOFactory.MapDAO.Insert(map);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat(ex.Message);
            }
        }
    }
}
