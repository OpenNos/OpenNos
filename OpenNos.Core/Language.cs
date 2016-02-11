using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.IO;

namespace OpenNos.Core
{
    public class Language
    {
        private static Language instance = null;
        private static readonly object myLock = new object();

        private Language() {
            CultureInfo newCultureInfo = new System.Globalization.CultureInfo(System.Configuration.ConfigurationManager.AppSettings["language"]);
            Thread.CurrentThread.CurrentCulture = newCultureInfo;
            Thread.CurrentThread.CurrentUICulture = newCultureInfo;
        }

        public static Language Instance
        {
            get
            {
                if (instance == null) instance = new Language();
                return instance;
            }
        }

        public string GetMessageFromKey(string message)
        {
            ResourceManager resourceManager = new ResourceManager(Assembly.GetEntryAssembly().GetName().Name + ".Resource.LocalizedResources", Assembly.GetEntryAssembly());
            if (resourceManager.GetString(message) != null && resourceManager.GetString(message) != "")
                return resourceManager.GetString(message);
            else
                return $"#<{message}>";
        }
    }
}
