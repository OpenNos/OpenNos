using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;
using System.Globalization;
using System.Xml;

namespace OpenNos.Handler
{

    public class ServerConfig : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            List<Server> liste = new List<Server>();
            foreach (XmlNode aServer in section.ChildNodes)
            {
                liste.Add(GetServer(aServer));
            }
            return liste;
        }

        public class Server
        {
            public string name { get; set; }
            public string WorldIp { get; set; }
            public int channelAmount { get; set; }
            public int WorldPort { get; set; }
   
        }
        public Server GetServer(XmlNode str)
        {
            Server result = new Server();
         
            result.name = str.Attributes["Name"].Value;
            result.WorldIp = str.Attributes["WorldIp"].Value;
            result.channelAmount = Convert.ToInt32(str.Attributes["channelAmount"].Value);
            result.WorldPort = Convert.ToInt32(str.Attributes["WorldPort"].Value);   
            return result;
        }
    }
 
}
