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

            Logger.Log.Warn(Language.Instance.GetMessageFromKey("NEED_TREE"));
            System.Console.BackgroundColor = System.ConsoleColor.Blue;
            System.Console.WriteLine("Root");
            System.Console.ResetColor();
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt\n-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_monster.txt\n-----MapIDData.dat\n-----monster.dat\n-----packet.txt");
            System.Console.BackgroundColor = System.ConsoleColor.Blue;
            System.Console.WriteLine("-----map");
            System.Console.ResetColor();
            System.Console.WriteLine("----------0\n----------1\n----------...");
            Logger.Log.Warn(Language.Instance.GetMessageFromKey("ENTER_PATH"));
            string folder = System.Console.ReadLine();     
            ImportFactory factory = new ImportFactory(folder);
           
            //factory.ImportItems();
            factory.ImportMaps();
            factory.ImportPortals();
            factory.ImportNpcs();
            factory.importShops();
            Thread.Sleep(5000);
        }
    }
}
