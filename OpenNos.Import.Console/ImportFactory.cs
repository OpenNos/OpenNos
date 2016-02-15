using OpenNos.Core;
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
    }
}
