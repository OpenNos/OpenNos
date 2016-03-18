using System;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// Stores service client informations to be used by an event.
    /// </summary>
    public class ServiceClientEventArgs : EventArgs
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ServiceClientEventArgs object.
        /// </summary>
        /// <param name="client">Client that is associated with this event</param>
        public ServiceClientEventArgs(IScsServiceClient client)
        {
            Client = client;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Client that is associated with this event.
        /// </summary>
        public IScsServiceClient Client { get; private set; }

        #endregion
    }
}