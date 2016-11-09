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

        /// <summary>
        /// Deserializes a string into a PacketBase
        /// </summary>
        /// <param name="packetContent">The content to deseralize</param>
        /// <param name="packetType">The type of the packet to deserialize to</param>
        /// <param name="includesKeepAliveIdentity">
        /// Include the keep alive identity or exclude it
        /// </param>
        /// <returns>The deserialized packet.</returns>
        public static object Deserialize(string packetContent, Type packetType, bool includesKeepAliveIdentity = false)
        {
            try
            {
                var serializationInformation = GetSerializationInformation(packetType);
                object deserializedPacket = Activator.CreateInstance(packetType); // reflection is bad, improve?

                deserializedPacket = Deserialize(packetContent, deserializedPacket, serializationInformation, includesKeepAliveIdentity);

                return deserializedPacket;
            }
            catch (Exception e)
            {
                Logger.Log.Warn($"The serialized packet has the wrong format. Packet: {packetContent}", e);
                return null;
            }
        }

        /// <summary>
        /// Deserializes a string into a PacketBase
        /// </summary>
        /// <param name="packetContent">The content to deseralize</param>
        /// <param name="includesKeepAliveIdentity">
        /// Include the keep alive identity or exclude it
        /// </param>
        /// <returns>The deserialized packet.</returns>
        public static TPacket Deserialize<TPacket>(string packetContent, bool includesKeepAliveIdentity = false)
            where TPacket : PacketBase
        {
            try
            {
                var serializationInformation = GetSerializationInformation(typeof(TPacket));
                TPacket deserializedPacket = Activator.CreateInstance<TPacket>(); // reflection is bad, improve?

                deserializedPacket = (TPacket)Deserialize(packetContent, deserializedPacket, serializationInformation, includesKeepAliveIdentity);

                return deserializedPacket;
            }
            catch (Exception e)
            {
                Logger.Log.Warn($"The serialized packet has the wrong format. Packet: {packetContent}", e);
                return null;
            }
        }

        /// <summary>
        /// Initializes the PacketFactory and generates the serialization informations based on the
        /// given BaseType.
        /// </summary>
        /// <typeparam name="TBaseType">The BaseType to generate serialization informations</typeparam>
        public static void Initialize<TBaseType>()
                    where TBaseType : PacketBase
        {
            if (!IsInitialized)
            {
                GenerateSerializationInformations<TBaseType>();
                IsInitialized = true;
            }
        }

        /// <summary>
        /// Serializes a PacketBase to string.
        /// </summary>
        /// <typeparam name="TPacket">The type of the PacketBase</typeparam>
        /// <param name="packet">The object reference of the PacketBase</param>
        /// <returns>The serialized string.</returns>
        public static string Serialize<TPacket>(TPacket packet)
                                    where TPacket : PacketBase
        {
            try
            {
                // load pregenerated serialization information
                var serializationInformation = GetSerializationInformation(packet.GetType());

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
                    deserializedPacket += DeserializeValue(packetBasePropertyInfo.Value.PropertyType, packetBasePropertyInfo.Value.GetValue(packet), packetBasePropertyInfo.Key);

                    // check if the value should be serialized to end
                    if (packetBasePropertyInfo.Key.SerializeToEnd)
                    {
                        // we reached the end
                        break;
                    }

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

        private static object Deserialize(string packetContent, object deserializedPacket, KeyValuePair<Tuple<Type, String>,
                                                                                    Dictionary<PacketIndexAttribute, PropertyInfo>> serializationInformation, bool includesKeepAliveIdentity)
        {
            MatchCollection matches = Regex.Matches(packetContent, @"([^\s]+[\.][^\s]+[\s]?)+((?=\s)|$)|([^\s]+)((?=\s)|$)");

            if (matches.Count > 0)
            {
                foreach (var packetBasePropertyInfo in serializationInformation.Value)
                {
                    int currentIndex = packetBasePropertyInfo.Key.Index + (includesKeepAliveIdentity ? 2 : 1); // adding 2 because we need to skip incrementing number and packet header

                    if (currentIndex < matches.Count)
                    {
                        if (packetBasePropertyInfo.Key.SerializeToEnd)
                        {
                            // get the value to the end and stop deserialization
                            string valueToEnd = packetContent.Substring(matches[currentIndex].Index, packetContent.Length - matches[currentIndex].Index);
                            packetBasePropertyInfo.Value.SetValue(deserializedPacket, SerializeValue(packetBasePropertyInfo.Value.PropertyType, valueToEnd, packetBasePropertyInfo.Key));
                            break;
                        }

                        string currentValue = matches[currentIndex].Value;

                        // set the value & convert currentValue
                        if (currentValue != null)
                        {
                            packetBasePropertyInfo.Value.SetValue(deserializedPacket, SerializeValue(packetBasePropertyInfo.Value.PropertyType, currentValue, packetBasePropertyInfo.Key));
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

        /// <summary> Converts for instance List<byte?> to -1.12.1.8.-1.-1.-1.-1.-1 </summary> <param
        /// name="listValues">Values in List of simple type.</param> <param name="propertyType">The
        /// simple type.</param> <returns></returns>
        private static string DeserializeSimpleList(IList listValues, Type propertyType)
        {
            string resultListPacket = String.Empty;
            int listValueCount = listValues.Count;
            if (listValueCount > 0)
            {
                resultListPacket += DeserializeValue(propertyType.GenericTypeArguments[0], listValues[0]);

                for (int i = 1; i < listValueCount; i++)
                {
                    resultListPacket += $".{DeserializeValue(propertyType.GenericTypeArguments[0], listValues[i]).Replace(" ", "")}";
                }
            }

            return resultListPacket;
        }

        private static string DeserializeSubpacket(object value, KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> subpacketSerializationInfo, bool isReturnPacket)
        {
            string serializedSubpacket = isReturnPacket ? $" #{subpacketSerializationInfo.Key.Item2}^" : " ";

            // iterate thru configure subpacket properties
            foreach (var subpacketPropertyInfo in subpacketSerializationInfo.Value)
            {
                // first element
                if (!(subpacketPropertyInfo.Key.Index == 0))
                {
                    serializedSubpacket += isReturnPacket ? "^" : ".";
                }

                serializedSubpacket += DeserializeValue(subpacketPropertyInfo.Value.PropertyType, subpacketPropertyInfo.Value.GetValue(value)).Replace(" ", "");
            }

            return serializedSubpacket;
        }

        private static string DeserializeSubpackets(IList listValues, Type packetBasePropertyType)
        {
            string serializedSubPacket = String.Empty;
            var subpacketSerializationInfo = GetSerializationInformation(packetBasePropertyType.GetGenericArguments()[0]);

            if (listValues.Count > 0)
            {
                foreach (object listValue in listValues)
                {
                    serializedSubPacket += DeserializeSubpacket(listValue, subpacketSerializationInfo, false);
                }
            }

            return serializedSubPacket;
        }

        private static string DeserializeValue(Type propertyType, object value, PacketIndexAttribute packetIndexAttribute = null)
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
                else if (propertyType.BaseType.Equals(typeof(PacketBase)))
                {
                    var subpacketSerializationInfo = GetSerializationInformation(propertyType);
                    return DeserializeSubpacket(value, subpacketSerializationInfo, packetIndexAttribute?.IsReturnPacket ?? false);
                }
                else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))
                    && propertyType.GenericTypeArguments[0].BaseType.Equals(typeof(PacketBase)))
                {
                    return DeserializeSubpackets((IList)value, propertyType);
                }
                else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) //simple list
                {
                    return DeserializeSimpleList((IList)value, propertyType);
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
                // add to serialization informations
                KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> serializationInformations =
                    GenerateSerializationInformations(packetBaseType);
            }
        }

        private static KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> GenerateSerializationInformations(Type serializationType)
        {
            string header = serializationType.GetCustomAttribute<PacketHeaderAttribute>()?.Identification;

            if (String.IsNullOrEmpty(header))
            {
                throw new Exception($"Packet header cannot be empty. PacketType: {serializationType.Name}");
            }

            Dictionary<PacketIndexAttribute, PropertyInfo> packetsForPacketDefinition = new Dictionary<PacketIndexAttribute, PropertyInfo>();

            foreach (PropertyInfo packetBasePropertyInfo in serializationType.GetProperties().Where(x => x.GetCustomAttributes(false).OfType<PacketIndexAttribute>().Any()))
            {
                PacketIndexAttribute indexAttribute = packetBasePropertyInfo.GetCustomAttributes(false).OfType<PacketIndexAttribute>().FirstOrDefault();

                if (indexAttribute != null)
                {
                    packetsForPacketDefinition.Add(indexAttribute, packetBasePropertyInfo);
                }
            }

            // order by index
            packetsForPacketDefinition.OrderBy(p => p.Key.Index);

            KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> serializationInformatin = new KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>>(new Tuple<Type, String>(serializationType, header), packetsForPacketDefinition);
            _packetSerializationInformations.Add(serializationInformatin.Key, serializationInformatin.Value);

            return serializationInformatin;
        }

        private static KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> GetSerializationInformation(Type serializationType)
        {
            return _packetSerializationInformations.Any(si => si.Key.Item1 == serializationType)
                                              ? _packetSerializationInformations.SingleOrDefault(si => si.Key.Item1 == serializationType)
                                              : GenerateSerializationInformations(serializationType); // generic runtime serialization parameter generation
        }

        /// <summary> Converts for instance -1.12.1.8.-1.-1.-1.-1.-1 to eg. List<byte?> </summary>
        /// <param name="currentValues">String to convert</param> <param name="genericListType">Type
        /// of the property to convert</param> <returns>The string as converted List</returns>
        private static IList SerializeSimpleList(string currentValues, Type genericListType)
        {
            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(genericListType), genericListType);
            IEnumerable<String> splittedValues = currentValues.Split('.');

            foreach (string currentValue in splittedValues)
            {
                object value = SerializeValue(genericListType.GenericTypeArguments[0], currentValue, null);
                subpackets.Add(value);
            }

            return subpackets;
        }

        private static object SerializeSubpacket(string currentSubValues, Type packetBasePropertyType, KeyValuePair<Tuple<Type, String>, Dictionary<PacketIndexAttribute, PropertyInfo>> subpacketSerializationInfo, bool isReturnPacket = false)
        {
            string[] subpacketValues = currentSubValues.Split(isReturnPacket ? '^' : '.');
            var newSubpacket = Activator.CreateInstance(packetBasePropertyType);

            foreach (var subpacketPropertyInfo in subpacketSerializationInfo.Value)
            {
                int currentSubIndex = isReturnPacket ? subpacketPropertyInfo.Key.Index + 1 : subpacketPropertyInfo.Key.Index; // return packets do include header
                string currentSubValue = subpacketValues[currentSubIndex];

                subpacketPropertyInfo.Value.SetValue(newSubpacket, SerializeValue(subpacketPropertyInfo.Value.PropertyType, currentSubValue, subpacketPropertyInfo.Key));
            }

            return newSubpacket;
        }

        /// <summary> Converts a Sublist of Packets, For instance 0.4903.5.0.0 2.340.0.0.0
        /// 3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 to List<EquipSubPacket> </summary>
        /// <param name="currentValue">The value as String</param> <param
        /// name="packetBasePropertyType">Type of the Property to convert to</param> <returns></returns>
        private static IList SerializeSubpackets(string currentValue, Type packetBasePropertyType)
        {
            IList subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(packetBasePropertyType), packetBasePropertyType);
            IEnumerable<String> splittedSubpackets = currentValue.Split(' ');
            Type subPacketType = packetBasePropertyType.GetGenericArguments()[0];
            var subpacketSerializationInfo = GetSerializationInformation(subPacketType);

            foreach (string subpacket in splittedSubpackets)
            {
                subpackets.Add(SerializeSubpacket(subpacket, subPacketType, subpacketSerializationInfo));
            }

            return subpackets;
        }

        private static object SerializeValue(Type packetPropertyType, string currentValue, PacketIndexAttribute packetIndexAttribute)
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
            else if (packetPropertyType.Equals(typeof(bool))) // handle boolean values
            {
                return currentValue == "0" ? false : true;
            }
            else if (packetPropertyType.BaseType.Equals(typeof(PacketBase))) // subpacket
            {
                var subpacketSerializationInfo = GetSerializationInformation(packetPropertyType);
                return SerializeSubpacket(currentValue, packetPropertyType, subpacketSerializationInfo, packetIndexAttribute?.IsReturnPacket ?? false);
            }
            else if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) // subpacket list
                && packetPropertyType.GenericTypeArguments[0].BaseType.Equals(typeof(PacketBase)))
            {
                return SerializeSubpackets(currentValue, packetPropertyType);
            }
            else if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) // simple list
            {
                return SerializeSimpleList(currentValue, packetPropertyType);
            }
            else if (Nullable.GetUnderlyingType(packetPropertyType) != null && String.IsNullOrEmpty(currentValue)) // empty nullable value
            {
                return null;
            }
            else if (Nullable.GetUnderlyingType(packetPropertyType) != null) // nullable value
            {
                return Convert.ChangeType(currentValue, packetPropertyType.GenericTypeArguments[0]);
            }
            else
            {
                return Convert.ChangeType(currentValue, packetPropertyType); // cast to specified type
            }
        }

        #endregion
    }
}