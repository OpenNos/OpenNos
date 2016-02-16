using log4net;
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace OpenNos.Import.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            System.Console.Title = $"OpenNos Import Console v{fileVersionInfo.ProductVersion}";
            System.Console.WriteLine("===============================================================================\n"
                             + $"                 IMPORT CONSOLE VERSION {fileVersionInfo.ProductVersion} by OpenNos Team\n" +
                             "===============================================================================\n");

            DataAccessHelper.Initialize();
            string folder = System.Console.ReadLine();     
            ImportFactory factory = new ImportFactory(folder);
            //factory.ImportItems();
            factory.ImportMaps();
            factory.ImportPortals();
            factory.ImportNpc();
            Thread.Sleep(5000);
        }
    }
}
