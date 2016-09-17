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

        private static Dictionary<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> _packetSerializationInformations;

        #endregion

        #region Properties

        public static bool IsInitialized { get; set; }

        #endregion

        #region Methods

        public static string Deserialize<TPacket>(TPacket packet)
            where TPacket : PacketBase
        {
            //load pregenerated serialization information
            var serializationInformation = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(typeof(TPacket)));

            string deserializedPacket = serializationInformation.Key.Item2; //set header

            int iterator = 0;
            foreach (var packetBasePropertyInfo in serializationInformation.Value)
            {
                //check if we need to add a non mapped value or a mapped
                if (packetBasePropertyInfo.Key.Index > iterator)
                {
                    deserializedPacket += " 0";
                }
                else
                {
                    if (packetBasePropertyInfo.Value.PropertyType.BaseType.Equals(typeof(Enum))) //enum should be casted to number
                    {
                        deserializedPacket += String.Format(" {0}", Convert.ToInt16(packetBasePropertyInfo.Value.GetValue(packet)));
                    }
                    else if (packetBasePropertyInfo.Key.HasStringOffset)
                    {
                        deserializedPacket += String.Format(" {0}", String.Format("{0} -", packetBasePropertyInfo.Value.GetValue(packet)));
                    }
                    else
                    {
                        deserializedPacket += String.Format(" {0}", packetBasePropertyInfo.Value.GetValue(packet));
                    }
                }

                iterator++;
            }

            return deserializedPacket;
        }

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
            var serializationInformation = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(typeof(TPacket)));
            TPacket deserializedPacket = Activator.CreateInstance<TPacket>(); //reflection is bad, improve?

            MatchCollection matches = Regex.Matches(packetContent, @"([\d\w]+)((?=\s)|$)|([\d\w]*\.[\d\w]\s*)+((?=\s)|$)");

            if (matches.Count > 0)
            {
                foreach (var packetBasePropertyInfo in serializationInformation.Value)
                {
                    int currentIndex = packetBasePropertyInfo.Key.Index + 2; //adding 2 because we need to skip incrementing number and packet header
                    string currentValue = matches[currentIndex].Value;

                    //check for offset and remove it
                    if (packetBasePropertyInfo.Key.HasStringOffset)
                    {
                        currentValue = currentValue.TrimEnd(' ', '-');
                    }

                    if (currentValue.Contains("."))
                    {
                        //currentvalue is list, check if property is also a list
                        if (typeof(IList).IsAssignableFrom(packetBasePropertyInfo.Value.PropertyType))
                        {
                            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(packetBasePropertyInfo.Value.PropertyType), packetBasePropertyInfo.Value.PropertyType);
                            IEnumerable<String> splittedSubpackets = currentValue.Split(' ');
                            Type subPacketType = packetBasePropertyInfo.Value.PropertyType.GetGenericArguments()[0];
                            var subpacketSerializationInfo = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(subPacketType));

                            foreach (string subpacket in splittedSubpackets)
                            {
                                string[] subpacketValues = subpacket.Split('.');
                                var newSubpacket = Activator.CreateInstance(subPacketType);

                                foreach (var subpacketPropertyInfo in subpacketSerializationInfo.Value)
                                {
                                    int currentSubIndex = subpacketPropertyInfo.Key.Index;
                                    string currentSubValue = subpacketValues[currentSubIndex];

                                    if (packetBasePropertyInfo.Value.PropertyType.BaseType.Equals(typeof(Enum))) //enum should be casted to number
                                    {
                                        subpacketPropertyInfo.Value.SetValue(newSubpacket, Enum.Parse(subpacketPropertyInfo.Value.PropertyType, currentSubValue));
                                    }
                                    else
                                    {
                                        subpacketPropertyInfo.Value.SetValue(newSubpacket, Convert.ChangeType(currentSubValue, subpacketPropertyInfo.Value.PropertyType));
                                    }
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
                        if (packetBasePropertyInfo.Value.PropertyType.BaseType.Equals(typeof(Enum))) //enum should be casted to number
                        {
                            packetBasePropertyInfo.Value.SetValue(deserializedPacket, Enum.Parse(packetBasePropertyInfo.Value.PropertyType, currentValue));
                        }
                        else
                        {
                            packetBasePropertyInfo.Value.SetValue(deserializedPacket, Convert.ChangeType(currentValue, packetBasePropertyInfo.Value.PropertyType));
                        }
                    }
                }
            }

            return deserializedPacket;
        }

        private static void GenerateSerializationInformations<TPacketBase>()
            where TPacketBase : PacketBase
        {
            _packetSerializationInformations = new Dictionary<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>>();

            //Iterate thru all PacketBase implementations
            foreach (Type packetBaseType in typeof(TPacketBase).Assembly.GetTypes().Where(p => !p.IsInterface && typeof(TPacketBase).BaseType.IsAssignableFrom(p)))
            {
                string header = packetBaseType.GetCustomAttribute<HeaderAttribute>()?.Identification;
                Dictionary<PacketIndexAttribute, PropertyInfo> PacketsForPacketDefinition = new Dictionary<PacketIndexAttribute, PropertyInfo>();

                foreach (PropertyInfo packetBasePropertyInfo in packetBaseType.GetProperties().Where(x => x.GetCustomAttributes(false).OfType<PacketIndexAttribute>().Any()))
                {
                    PacketIndexAttribute indexAttribute = packetBasePropertyInfo.GetCustomAttributes(false).OfType<PacketIndexAttribute>().FirstOrDefault();

                    if (indexAttribute != null)
                    {
                        PacketsForPacketDefinition.Add(indexAttribute, packetBasePropertyInfo);
                    }
                }

                //order by index
                PacketsForPacketDefinition.OrderBy(p => p.Key.Index);

                //add to serialization informations
                _packetSerializationInformations.Add(new Tuple<Type, String>(packetBaseType, header), PacketsForPacketDefinition);
            }
        }

        #endregion
    }
}