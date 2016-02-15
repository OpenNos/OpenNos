using OpenNos.DAL.EF.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Import.Console
{
    public class Program
    {
        static void Main(string[] args)
        {

            string folder = System.Console.ReadLine();

            DataAccessHelper.Initialize();

            ImportFactory factory = new ImportFactory(folder);
            //factory.ImportItems();
            factory.ImportMaps();
        }
    }
}
