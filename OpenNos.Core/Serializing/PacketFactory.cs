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
            try
            {
                // load pregenerated serialization information
                var serializationInformation = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(packet.GetType()));

                string deserializedPacket = serializationInformation.Key.Item2; // set header

                int lastIndex = 0;
                foreach (var packetBasePropertyInfo in serializationInformation.Value)
                {
                    // check if we need to add a non mapped values (pseudovalues)
                    if (packetBasePropertyInfo.Key.Index > lastIndex + 1)
                    {
                        int amountOfEmptyValuesToAdd = packetBasePropertyInfo.Key.Index - (lastIndex + 1);

                        for (int i = 0; i < amountOfEmptyValuesToAdd; i++)
                        {
                            deserializedPacket += " 0";
                        }
                    }

                    // add value for current configuration
                    deserializedPacket += ConvertValueBack(packetBasePropertyInfo.Value.PropertyType, packetBasePropertyInfo.Value.GetValue(packet));

                    // set new index
                    lastIndex = packetBasePropertyInfo.Key.Index;
                }

                return deserializedPacket;
            }
            catch (Exception e)
            {
                Logger.Log.Warn("Wrong Packet Format!", e);
                return String.Empty;
            }
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

        public static TPacket Serialize<TPacket>(string packetContent, bool includesKeepAliveIdentity = false)
            where TPacket : PacketBase
        {
            try
            {
                var serializationInformation = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(typeof(TPacket)));
                TPacket deserializedPacket = Activator.CreateInstance<TPacket>(); // reflection is bad, improve?

                MatchCollection matches = Regex.Matches(packetContent, @"([^\s]+[\.\^][^\s]+[\s]?)+((?=\s)|$)|([^\s]+)((?=\s)|$)");

                if (matches.Count > 0)
                {
                    foreach (var packetBasePropertyInfo in serializationInformation.Value)
                    {
                        int currentIndex = packetBasePropertyInfo.Key.Index + (includesKeepAliveIdentity ? 2 : 1); // adding 2 because we need to skip incrementing number and packet header

                        if (currentIndex < matches.Count)
                        {
                            string currentValue = matches[currentIndex].Value;

                            // set the value & convert currentValue
                            if (currentValue != null)
                            {
                                packetBasePropertyInfo.Value.SetValue(deserializedPacket, ConvertValue(packetBasePropertyInfo.Value.PropertyType, currentValue));
                            }
                            else
                            {
                                packetBasePropertyInfo.Value.SetValue(deserializedPacket, Activator.CreateInstance(packetBasePropertyInfo.Value.PropertyType));
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return deserializedPacket;
            }
            catch (Exception e)
            {
                Logger.Log.Warn($"The serialized packet has the wrong format. Packet: {packetContent}", e);
                return null;
            }
        }

        /// <summary> Converts for instance -1.12.1.8.-1.-1.-1.-1.-1 to eg. List<byte?> </summary>
        /// <param name="currentValues">String to convert</param> <param name="genericListType">Type
        /// of the property to convert</param> <returns>The string as converted List</returns>
        private static IList ConvertSimpleList(string currentValues, Type genericListType)
        {
            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(genericListType), genericListType);
            IEnumerable<String> splittedValues = currentValues.Split('.');

            foreach (string currentValue in splittedValues)
            {
                object value = ConvertValue(genericListType.GenericTypeArguments[0], currentValue);
                subpackets.Add(value);
            }

            return subpackets;
        }

        /// <summary> Converts for instance List<byte?> to -1.12.1.8.-1.-1.-1.-1.-1 </summary> <param
        /// name="listValues">Values in List of simple type.</param> <param name="propertyType">The
        /// simple type.</param> <returns></returns>
        private static string ConvertSimpleListBack(IList listValues, Type propertyType)
        {
            string resultListPacket = String.Empty;
            int listValueCount = listValues.Count;
            if (listValueCount > 0)
            {
                resultListPacket += ConvertValueBack(propertyType.GenericTypeArguments[0], listValues[0]);

                for (int i = 1; i < listValueCount; i++)
                {
                    resultListPacket += $".{ConvertValueBack(propertyType.GenericTypeArguments[0], listValues[i]).Replace(" ", "")}";
                }
            }

            return resultListPacket;
        }

        /// <summary> Converts a Sublist of Packets, For instance 0.4903.5.0.0 2.340.0.0.0
        /// 3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 to List<EquipSubPacket> </summary>
        /// <param name="currentValue">The value as String</param> <param
        /// name="packetBasePropertyType">Type of the Property to convert to</param> <returns></returns>
        private static IList ConvertSubList(string currentValue, Type packetBasePropertyType)
        {
            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(packetBasePropertyType), packetBasePropertyType);
            IEnumerable<String> splittedSubpackets = currentValue.Split(' ');
            Type subPacketType = packetBasePropertyType.GetGenericArguments()[0];
            var subpacketSerializationInfo = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(subPacketType));

            foreach (string subpacket in splittedSubpackets)
            {
                string[] subpacketValues = subpacket.Split('.');
                var newSubpacket = Activator.CreateInstance(subPacketType);

                foreach (var subpacketPropertyInfo in subpacketSerializationInfo.Value)
                {
                    int currentSubIndex = subpacketPropertyInfo.Key.Index;
                    string currentSubValue = subpacketValues[currentSubIndex];

                    subpacketPropertyInfo.Value.SetValue(newSubpacket, ConvertValue(subpacketPropertyInfo.Value.PropertyType, currentSubValue));
                }

                subpackets.Add(newSubpacket);
            }

            return subpackets;
        }

        private static string ConvertSubListBack(IList listValues, Type packetBasePropertyType)
        {
            string serializedSubPacket = String.Empty;
            var subpacketSerializationInfo = _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1.Equals(packetBasePropertyType.GetGenericArguments()[0]));

            if (listValues.Count > 0)
            {
                foreach (var listValue in listValues)
                {
                    serializedSubPacket += " ";

                    // iterate thru configure subpacket properties
                    foreach (var subpacketPropertyInfo in subpacketSerializationInfo.Value)
                    {
                        // first element
                        if (!(subpacketPropertyInfo.Key.Index == 0))
                        {
                            serializedSubPacket += ".";
                        }

                        serializedSubPacket += ConvertValueBack(subpacketPropertyInfo.Value.PropertyType, subpacketPropertyInfo.Value.GetValue(listValue)).Replace(" ", "");
                    }
                }
            }

            return serializedSubPacket;
        }

        private static object ConvertValue(Type packetPropertyType, string currentValue)
        {
            // check for empty value and cast it to null
            if (currentValue == "-1" || currentValue == "-")
            {
                currentValue = null;
            }

            // enum should be casted to number
            if (packetPropertyType.BaseType != null && packetPropertyType.BaseType.Equals(typeof(Enum)))
            {
                object convertedValue = null;
                try
                {
                    convertedValue = Enum.Parse(packetPropertyType, currentValue);
                }
                catch (Exception)
                {
                    Logger.Log.Warn($"Could not convert value {currentValue} to type {packetPropertyType.Name}");
                }

                return convertedValue;
            }
            else if (packetPropertyType.Equals(typeof(bool)))
            {
                return currentValue == "0" ? false : true;
            }
            else if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) && packetPropertyType.GenericTypeArguments[0].BaseType.Equals(typeof(PacketBase)))
            {
                return ConvertSubList(currentValue, packetPropertyType);
            }
            else if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
            {
                // check for IList but not IList<PacketBase> -> Simple lists
                return ConvertSimpleList(currentValue, packetPropertyType);
            }
            else if (Nullable.GetUnderlyingType(packetPropertyType) != null && String.IsNullOrEmpty(currentValue))
            {
                return null;
            }
            else if (Nullable.GetUnderlyingType(packetPropertyType) != null)
            {
                return Convert.ChangeType(currentValue, packetPropertyType.GenericTypeArguments[0]);
            }
            else
            {
                return Convert.ChangeType(currentValue, packetPropertyType);
            }
        }

        private static string ConvertValueBack(Type propertyType, object value)
        {
            if (propertyType != null)
            {
                // check for nullable without value or string
                if (propertyType.Equals(typeof(string)) && String.IsNullOrEmpty(Convert.ToString(value)))
                {
                    return " -";
                }
                if (Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(Convert.ToString(value)))
                {
                    return " -1";
                }

                // enum should be casted to number
                if (propertyType.BaseType != null && propertyType.BaseType.Equals(typeof(Enum)))
                {
                    return String.Format(" {0}", Convert.ToInt16(value));
                }
                else if (propertyType.Equals(typeof(bool)))
                {
                    // bool is 0 or 1 not True or False
                    return Convert.ToBoolean(value) ? " 1" : " 0";
                }
                else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))
                    && propertyType.GenericTypeArguments[0].BaseType.Equals(typeof(PacketBase)))
                {
                    return ConvertSubListBack((IList)value, propertyType);
                }
                else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
                {
                    // check for IList but not IList<PacketBase> -> Simple lists
                    return ConvertSimpleListBack((IList)value, propertyType);
                }
                else
                {
                    return String.Format(" {0}", value);
                }
            }

            return String.Empty;
        }

        private static void GenerateSerializationInformations<TPacketBase>()
            where TPacketBase : PacketBase
        {
            _packetSerializationInformations = new Dictionary<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>>();

            // Iterate thru all PacketBase implementations
            foreach (Type packetBaseType in typeof(TPacketBase).Assembly.GetTypes().Where(p => !p.IsInterface && typeof(TPacketBase).BaseType.IsAssignableFrom(p)))
            {
                string header = packetBaseType.GetCustomAttribute<HeaderAttribute>()?.Identification;

                if (String.IsNullOrEmpty(header))
                {
                    throw new Exception($"Packet header cannot be empty. PacketType: {packetBaseType.Name}");
                }

                Dictionary<PacketIndexAttribute, PropertyInfo> packetsForPacketDefinition = new Dictionary<PacketIndexAttribute, PropertyInfo>();

                foreach (PropertyInfo packetBasePropertyInfo in packetBaseType.GetProperties().Where(x => x.GetCustomAttributes(false).OfType<PacketIndexAttribute>().Any()))
                {
                    PacketIndexAttribute indexAttribute = packetBasePropertyInfo.GetCustomAttributes(false).OfType<PacketIndexAttribute>().FirstOrDefault();

                    if (indexAttribute != null)
                    {
                        packetsForPacketDefinition.Add(indexAttribute, packetBasePropertyInfo);
                    }
                }

                // order by index
                packetsForPacketDefinition.OrderBy(p => p.Key.Index);

                // add to serialization informations
                _packetSerializationInformations.Add(new Tuple<Type, String>(packetBaseType, header), packetsForPacketDefinition);
            }
        }

        #endregion
    }
}