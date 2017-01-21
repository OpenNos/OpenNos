using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using OpenNos.Domain;

namespace OpenNos.GameObject
{

    public class EventSchedule : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            List<Schedule> liste = new List<Schedule>();
            foreach (XmlNode aSchedule in section.ChildNodes)
            {
                liste.Add(GetSchedule(aSchedule));
            }
            return liste;
        }

        public Schedule GetSchedule(XmlNode str)
        {
            Schedule result = new Schedule();

            result.Event = (EventType)Enum.Parse(typeof(EventType),str.Attributes["event"].Value);
            result.Time = TimeSpan.Parse(str.Attributes["time"].Value);
            return result;
        }
    }

}