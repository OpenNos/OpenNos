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
                    deserializedPacket += ConvertPacketValueBack(packetBasePropertyInfo.Value.PropertyType, packetBasePropertyInfo.Value.GetValue(packet));
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

            MatchCollection matches = Regex.Matches(packetContent, @"([^\s]+[\.\^][^\s]+[\s]?)+((?=\s)|$)|([^\s]+)((?=\s)|$)");

            if (matches.Count > 0)
            {
                foreach (var packetBasePropertyInfo in serializationInformation.Value)
                {
                    int currentIndex = packetBasePropertyInfo.Key.Index + 2; //adding 2 because we need to skip incrementing number and packet header
                    string currentValue = matches[currentIndex].Value;

                    //set the value & convert currentValue
                    packetBasePropertyInfo.Value.SetValue(deserializedPacket, ConvertPacketValue(packetBasePropertyInfo.Value.PropertyType, currentValue));
                }
            }

            return deserializedPacket;
        }

        private static IList ConvertAdvancedList(string currentValue, Type packetBasePropertyType)
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

                    subpacketPropertyInfo.Value.SetValue(newSubpacket, ConvertPacketValue(subpacketPropertyInfo.Value.PropertyType, currentSubValue));
                }

                subpackets.Add(newSubpacket);
            }

            return subpackets;
        }

        private static object ConvertPacketValue(Type packetPropertyType, string currentValue)
        {
            if (currentValue == "-1" || currentValue == "-")//check for empty value and cast it to null
            {
                currentValue = null;
            }

            if (packetPropertyType.BaseType != null && packetPropertyType.BaseType.Equals(typeof(Enum))) //enum should be casted to number
            {
                return Enum.Parse(packetPropertyType, currentValue);
            }
            else if (packetPropertyType.Equals(typeof(bool)))
            {
                return currentValue == "0" ? false : true;
            }
            else if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) && packetPropertyType.GenericTypeArguments[0].BaseType.Equals(typeof(PacketBase)))
            {
                return ConvertAdvancedList(currentValue, packetPropertyType);
            }
            else if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) //check for IList but not IList<PacketBase> -> Simple lists
            {
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

        private static string ConvertPacketValueBack(Type propertyType, object value)
        {
            //check for nullable without value or string
            if (propertyType.Equals(typeof(string)) && String.IsNullOrEmpty(Convert.ToString(value)))
            {
                return " -";
            }
            if (Nullable.GetUnderlyingType(propertyType) != null && String.IsNullOrEmpty(Convert.ToString(value)))
            {
                return " -1";
            }
            if (propertyType.BaseType != null && propertyType.BaseType.Equals(typeof(Enum))) //enum should be casted to number
            {
                return String.Format(" {0}", Convert.ToInt16(value));
            }
            else if (propertyType.Equals(typeof(bool))) //vool is 0 or 1 not True or False
            {
                return Convert.ToBoolean(value) ? " 1" : " 0";
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))
                && propertyType.GenericTypeArguments[0].BaseType.Equals(typeof(PacketBase)))
            {
                //TODO Advanced List
                return String.Empty;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) //check for IList but not IList<PacketBase> -> Simple lists
            {
                return ConvertSimpleListBack(value, propertyType);
            }
            else
            {
                return String.Format(" {0}", value);
            }
        }

        /// <summary>
        /// Converts for instance -1.12.1.8.-1.-1.-1.-1.-1
        /// </summary>
        /// <param name="currentValues">String to convert</param>
        /// <param name="genericListType">Values to convert</param>
        /// <returns></returns>
        private static IList ConvertSimpleList(string currentValues, Type genericListType)
        {
            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(genericListType), genericListType);
            IEnumerable<String> splittedValues = currentValues.Split('.');

            foreach (string currentValue in splittedValues)
            {
                object value = ConvertPacketValue(genericListType.GenericTypeArguments[0], currentValue);
                subpackets.Add(value);
            }

            return subpackets;
        }

        private static string ConvertSimpleListBack(object value, Type propertyType)
        {
            IList listValues = (IList)value;
            string resultListPacket = String.Empty;
            int listValueCount = listValues.Count;
            if (listValueCount > 0)
            {
                resultListPacket += ConvertPacketValueBack(propertyType.GenericTypeArguments[0], listValues[0]);

                for (int i = 1; i < listValueCount; i++)
                {
                    resultListPacket += $".{ConvertPacketValueBack(propertyType.GenericTypeArguments[0], listValues[i]).Replace(" ", "")}";
                }
            }

            return resultListPacket;
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