using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenNos.Core
{
    public static class PacketFactory
    {
        #region Members

        private static Dictionary<Type, Dictionary<IndexAttribute, PropertyInfo>> _packetSerializationInformations;

        #endregion

        #region Properties

        public static bool IsInitialized { get; set; }

        #endregion

        #region Methods

        public static void Initialize<TBaseType>()
            where TBaseType : PacketBase
        {
            if (!IsInitialized)
            {
                GenerateSerializationInformations<TBaseType>();
                IsInitialized = true;
            }
        }

        public static TPacket Serialize<TPacket>(string packetContent)
                    where TPacket : PacketBase
        {
            packetContent = packetContent + " "; //hotfix

            var serializationInformation = _packetSerializationInformations.SingleOrDefault(si => si.Key.Equals(typeof(TPacket)));
            TPacket deserializedPacket = Activator.CreateInstance<TPacket>(); //reflection is bad, improve?

            MatchCollection matches = Regex.Matches(packetContent, @"([\d\w]+)(?=\s)|([\d\w]*\.[\d\w]\s*)+((?=\s)|$)");

            if (matches.Count > 0)
            {
                foreach (var packetBasePropertyInfo in serializationInformation.Value)
                {
                    int currentIndex = packetBasePropertyInfo.Key.Index + 2; //adding 2 because we need to skip incrementing number and packet header
                    string currentValue = matches[currentIndex].Value;

                    if (currentValue.Contains(".")) //NOT TESTED, SEEMS TO NEVER HAPPEN
                    {
                        //throw new Exception("Are your sure that you received a packet with a list inside?");

                        //currentvalue is list, check if property is also a list
                        if (typeof(IList).IsAssignableFrom(packetBasePropertyInfo.Value.PropertyType))
                        {
                            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(packetBasePropertyInfo.Value.PropertyType), packetBasePropertyInfo.Value.PropertyType);
                            IEnumerable<String> splittedSubpackets = currentValue.Split(' ');
                            Type subPacketType = packetBasePropertyInfo.Value.PropertyType.GetGenericArguments()[0];
                            var subpacketSerializationInfo = _packetSerializationInformations.SingleOrDefault(si => si.Key.Equals(subPacketType));

                            foreach (string subpacket in splittedSubpackets)
                            {
                                string[] subpacketValues = subpacket.Split('.');
                                var newSubpacket = Activator.CreateInstance(subPacketType);

                                foreach (var subpacketPropertyInfo in subpacketSerializationInfo.Value)
                                {
                                    int currentSubIndex = subpacketPropertyInfo.Key.Index;
                                    string currentSubValue = subpacketValues[currentSubIndex];

                                    subpacketPropertyInfo.Value.SetValue(newSubpacket, Convert.ChangeType(currentSubValue, subpacketPropertyInfo.Value.PropertyType));
                                }

                                subpackets.Add(newSubpacket);
                            }

                            packetBasePropertyInfo.Value.SetValue(deserializedPacket, Convert.ChangeType(subpackets, packetBasePropertyInfo.Value.PropertyType));
                        }
                    }
                    else
                    {
                        //simple value
                        //set the value & convert currentValue
                        packetBasePropertyInfo.Value.SetValue(deserializedPacket, Convert.ChangeType(currentValue, packetBasePropertyInfo.Value.PropertyType));
                    }
                }
            }

            return deserializedPacket;
        }

        private static void GenerateSerializationInformations<TBaseType>()
            where TBaseType : PacketBase
        {
            _packetSerializationInformations = new Dictionary<Type, Dictionary<IndexAttribute, PropertyInfo>>();

            //Iterate thru all PacketBase implementations
            foreach (Type packetBaseType in typeof(TBaseType).Assembly.GetTypes().Where(p => !p.IsInterface && typeof(TBaseType).BaseType.IsAssignableFrom(p)))
            {
                Dictionary<IndexAttribute, PropertyInfo> PacketsForPacketDefinition = new Dictionary<IndexAttribute, PropertyInfo>();

                foreach (PropertyInfo packetBasePropertyInfo in packetBaseType.GetProperties().Where(x => x.GetCustomAttributes(false).OfType<IndexAttribute>().Any()))
                {
                    IndexAttribute indexAttribute = packetBasePropertyInfo.GetCustomAttributes(false).OfType<IndexAttribute>().FirstOrDefault();

                    if (indexAttribute != null)
                    {
                        PacketsForPacketDefinition.Add(indexAttribute, packetBasePropertyInfo);
                    }
                }

                //order by index
                PacketsForPacketDefinition.OrderBy(p => p.Key.Index);

                //add to serialization informations
                _packetSerializationInformations.Add(packetBaseType, PacketsForPacketDefinition);
            }
        }

        #endregion
    }
}