using Hik.Communication.ScsServices.Service;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.Master.Library.Interface
{
    [ScsService(Version = "1.0.0.0")]
    public interface ICommunicationService
    {
        /// <summary>
        /// Authenticates a Client to the Service
        /// </summary>
        /// <param name="authKey">The private Authentication key</param>
        /// <returns>true if successful, else false</returns>
        bool Authenticate(string authKey);

        /// <summary>
        /// Checks if the Account is already connected
        /// </summary>
        /// <param name="accountId">Id of the Account</param>
        /// <returns></returns>
        bool IsAccountConnected(long accountId);

        /// <summary>
        /// Refreshes the Pulse Timer for the given account
        /// </summary>
        /// <param name="accountId">Id of the Account</param>
        void PulseAccount(long accountId);

        /// <summary>
        /// Checks if the Character is connected
        /// </summary>
        /// <param name="worldGroup">Name of the WorldGroup to look on</param>
        /// <param name="characterId">Id of the Character</param>
        /// <returns></returns>
        bool IsCharacterConnected(string worldGroup, long characterId);

        /// <summary>
        /// Updates the Bazaar on the given WorldGroup
        /// </summary>
        /// <param name="worldGroup">WorldGroup the entry should be update on</param>
        /// <param name="bazaarItemId">BazaarItemId that should be updated</param>
        void UpdateBazaar(string worldGroup, long bazaarItemId);

        /// <summary>
        /// Cleanup, used when rebooting the Server
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Registers the Login of the given Account and removes the permission to login
        /// </summary>
        /// <param name="worldId">World the Account connects to</param>
        /// <param name="accountId">Id of the connecting Account</param>
        /// <param name="sessionId">Id of the Session requesting the Login</param>
        /// <returns>true if the Login was successful, otherwise false</returns>
        bool ConnectAccount(Guid worldId, long accountId, long sessionId);

        /// <summary>
        /// Registers the Login of the given Character
        /// </summary>
        /// <param name="worldId">World the Character connects to</param>
        /// <param name="characterId">Id of the connecting Character </param>
        /// <returns>true if the Login was successful, otherwise false</returns>
        bool ConnectCharacter(Guid worldId, long characterId);

        /// <summary>
        /// Registers the Logout of the given Account
        /// </summary>
        /// <param name="accountId">Id of the disconnecting Account</param>
        void DisconnectAccount(long accountId);

        /// <summary>
        /// Registers the Logout of the given Character
        /// </summary>
        /// <param name="worldId">World the Character was connected to</param>
        /// <param name="characterId">Id of the disconnecting Character</param>
        void DisconnectCharacter(Guid worldId, long characterId);

        /// <summary>
        /// Gets the ChannelId by the given WorldId
        /// </summary>
        /// <param name="worldId">Id of the World</param>
        /// <returns>ChannelId on success, otherwise null</returns>
        int? GetChannelIdByWorldId(Guid worldId);


        /// <summary>
        /// Updates a Family on the given WorldGroup
        /// </summary>
        /// <param name="worldGroup">WorldGroup the Family should be updated on</param>
        /// <param name="familyId">Family that should be updated</param>
        void UpdateFamily(string worldGroup, long familyId);

        /// <summary>
        /// Checks if the Account is allowed to login
        /// </summary>
        /// <param name="accountId">Id of the Account</param>
        /// <param name="sessionId">Id of the Session that should be validated</param>
        /// <returns></returns>
        bool IsLoginPermitted(long accountId, long sessionId);

        /// <summary>
        /// Kicks a Session by their Id or Account
        /// </summary>
        /// <param name="accountId">Id of the Account</param>
        /// <param name="sessionId">Id of the Session</param>
        void KickSession(long? accountId, long? sessionId);

        /// <summary>
        /// Refreshes the Penalty Cache
        /// </summary>
        /// <param name="penaltyId">Id of the Penalty to refresh</param>
        void RefreshPenalty(int penaltyId);

        /// <summary>
        /// Registers the Account for Login
        /// </summary>
        /// <param name="accountId">Id of the Account to register</param>
        /// <param name="sessionId">Id of the Session to register</param>
        void RegisterAccountLogin(long accountId, long sessionId);

        /// <summary>
        /// Updates the Relations on the given WorldGroup
        /// </summary>
        /// <param name="worldGroup">WorldGroup the Relations should be updated on</param>
        /// <param name="relationId">Id of the Relation that should be updated</param>
        void UpdateRelation(string worldGroup, long relationId);

        /// <summary>
        /// Shutdown given WorldGroup or WorldServer
        /// </summary>
        /// <param name="worldGroup">WorldGroup that should be shut down</param>
        void Shutdown(string worldGroup);

        /// <summary>
        /// Registers a WorldServer
        /// </summary>
        /// <param name="worldServer">SerializableWorldServer object of the Server that should be registered</param>
        /// <returns>ChannelId on success, else null</returns>
        int? RegisterWorldServer(SerializableWorldServer worldServer);

        /// <summary>
        /// Generates the Stats from all Servers 
        /// </summary>
        /// <returns>the actual result</returns>
        IEnumerable<string> RetrieveServerStatistics();

        /// <summary>
        /// Generates the Channel Selection Packet
        /// </summary>
        /// <returns>the actual packet</returns>
        string RetrieveRegisteredWorldServers(long sessionId);

        /// <summary>
        /// Sends a Message to a specific Character
        /// </summary>
        /// <param name="message">The SCSCharacterMessage object containing all required informations</param>
        /// <returns>null if there was an error, otherwise the receiving ChannelId or -1, if the MessageType is a broadcast</returns>
        int? SendMessageToCharacter(SCSCharacterMessage message);

        /// <summary>
        /// Unregisters a previously registered World Server
        /// </summary>
        /// <param name="worldId">Id of the World to be unregistered</param>
        void UnregisterWorldServer(Guid worldId);
    }
}
