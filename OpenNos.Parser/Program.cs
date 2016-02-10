using log4net;
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.Title = ($"OpenNos Parser  v{0}");
            Console.WriteLine(("===============================================================================\n"
                             + $"                 PARSER VERSION {fileVersionInfo.ProductVersion} by OpenNos Team\n" +
                             "===============================================================================\n"));

            //initialize DB
            DataAccessHelper.Initialize();
           
        }
    }
}
