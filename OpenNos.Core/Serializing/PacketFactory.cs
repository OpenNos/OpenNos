using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public static TPacket Deserialize<TPacket>(string packetContent)
            where TPacket : PacketBase
        {
            var serializationInformation = _packetSerializationInformations.SingleOrDefault(si => si.Key.Equals(typeof(TPacket)));

            //Not done for now
            if (packetContent.Contains("."))
            {
                throw new Exception("PacketFactory not capable of arrays for now.");
            }

            TPacket deserializedPacket = Activator.CreateInstance<TPacket>(); //reflection is bad, improve?
            string[] splittedPacket = packetContent.Split(' ');

            foreach (var packetBasePropertyInfo in serializationInformation.Value)
            {
                int currentIndex = packetBasePropertyInfo.Key.Index + 2; //adding 2 because we need to skip incrementing number and packet header
                string currentValue = splittedPacket[currentIndex];

                //set the value & convert currentValue
                packetBasePropertyInfo.Value.SetValue(deserializedPacket, Convert.ChangeType(currentValue, packetBasePropertyInfo.Value.PropertyType));
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
                    IndexAttribute Packet = packetBasePropertyInfo.GetCustomAttributes(false).OfType<IndexAttribute>().FirstOrDefault();

                    if (Packet != null)
                    {
                        PacketsForPacketDefinition.Add(Packet, packetBasePropertyInfo);
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