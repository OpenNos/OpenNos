using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Threading;
namespace OpenNos.Core
{
    public class Language
    {
        public Language()
        {
            CultureInfo newCultureInfo = new System.Globalization.CultureInfo(System.Configuration.ConfigurationManager.AppSettings["language"]);
            Thread.CurrentThread.CurrentCulture = newCultureInfo;
            Thread.CurrentThread.CurrentUICulture = newCultureInfo;
        }
        public string i18n(string message)
        {
            
            ResourceManager resourceManager = new ResourceManager("OpenNos.Core.Resource.LocalizedResources", Assembly.GetExecutingAssembly());
            if (resourceManager.GetString(message) != null && resourceManager.GetString(message) != "")
                return resourceManager.GetString(message);
            else
                return String.Format("#{{0}}", message);
        }
    }
}
