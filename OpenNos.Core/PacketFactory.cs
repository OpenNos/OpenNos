using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class PacketFactory
    {

        #region Methods

        /// <summary>
        /// Create an Object of type T based on a string Packet.
        /// </summary>
        /// <typeparam name="T">Type of the object to Cast</typeparam>
        /// <param name="packet">Packet in string format.</param>
        /// <returns>Deserialized Packet.</returns>
        public static T Deserialize<T>(string packet)
        {
            T deserializedObject = Activator.CreateInstance<T>();

            string[] packetParts = packet.Split(' ');

            if (packetParts.Length > 1)
            {
                int packetIndex = 1;
                foreach (PropertyInfo property in deserializedObject.GetType().GetProperties())
                {
                    if (property.CanWrite)
                    {
                        // Get the type code so we can switch
                        System.TypeCode typeCode = System.Type.GetTypeCode(property.PropertyType);
                        try
                        {
                            switch (typeCode)
                            {
                                case TypeCode.Int32:
                                    property.SetValue(deserializedObject, Convert.ToInt32(packetParts[packetIndex]), null);
                                    break;
                                case TypeCode.Int64:
                                    property.SetValue(deserializedObject, Convert.ToInt64(packetParts[packetIndex]), null);
                                    break;
                                case TypeCode.String:
                                    property.SetValue(deserializedObject, packetParts[packetIndex], null);
                                    break;
                                default:
                                    property.SetValue(deserializedObject, packetParts[packetIndex], null);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.ErrorFormat($"Unable to set Property {property.Name} unsupported type {property.PropertyType}.");
                        }

                        packetIndex++;
                    }                    
                }
            }
            else
            {
                Logger.Log.ErrorFormat($"Could not deserilize Packet {packet}");
            }

            return deserializedObject;
        }

        #endregion
    }
}
