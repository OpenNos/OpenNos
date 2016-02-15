using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                string file = $"{_folder}\\map";
                DirectoryInfo dir = new DirectoryInfo(file);
                FileInfo[] fichiers = dir.GetFiles();


                foreach (FileInfo fichier in fichiers)
                {//TODO add name parse
                    MapDTO map = new MapDTO { Name = "", Music = 0, MapId = short.Parse(fichier.Name), Data = System.IO.File.ReadAllBytes(fichier.FullName) };
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
