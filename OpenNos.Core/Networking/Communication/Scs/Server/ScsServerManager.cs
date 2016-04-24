using System.Threading;

namespace OpenNos.Core.Networking.Communication.Scs.Server
{
    /// <summary>
    /// Provides some functionality that are used by servers.
    /// </summary>
    public static class ScsServerManager
    {
        #region Members

        /// <summary>
        /// Used to set an auto incremential unique identifier to clients.
        /// </summary>
        private static long _lastClientId;

        #endregion

        #region Methods

        /// <summary>
        /// Gets an unique number to be used as idenfitier of a client.
        /// </summary>
        /// <returns></returns>
        public static long GetClientId()
        {
            return Interlocked.Increment(ref _lastClientId);
        }

        #endregion
    }
}