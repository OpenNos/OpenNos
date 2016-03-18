using System;
using System.Runtime.Serialization;

namespace OpenNos.Core.Networking.Communication.Scs.Communication
{
    /// <summary>
    /// This application is thrown if communication is not expected state.
    /// </summary>
    [Serializable]
    public class CommunicationStateException : CommunicationException
    {
        #region Instantiation

        /// <summary>
        /// Contstructor.
        /// </summary>
        public CommunicationStateException()
        {
        }

        /// <summary>
        /// Contstructor for serializing.
        /// </summary>
        public CommunicationStateException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public CommunicationStateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public CommunicationStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}