using OpenNos.Core.Networking.Communication.ScsServices.Service;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.Master.Library;
using System;
using System.Collections.Generic;

namespace OpenNos.Master.Interface
{
    [ScsService(Version = "1.0.0.0")]
    public interface ICommunicationService
    {
        /// <summary>
        /// Checks if the Account is already connected
        /// </summary>
        /// <param name="accountId">Id of the Account</param>
        /// <returns></returns>
        bool IsAccountConnected(long accountId);

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
        /// Updates a Family on the given WorldGroup
        /// </summary>
        /// <param name="worldGroup">WorldGroup the Family should be updated on</param>
        /// <param name="familyId">Family that should be updated</param>
        void UpdateFamily(string worldGroup, long familyId);

        /// <summary>
        /// Checks if the Account is allowed to login and
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
        /// Registers a new WorldServer
        /// </summary>
        /// <param name="worldServer">WorldServer object of the Server that should be registered</param>
        /// <returns>ChannelId of the WorldServer</returns>
        int? RegisterWorldServer(WorldServer worldServer);

        /// <summary>
        /// Updates the Relations on the given WorldGroup
        /// </summary>
        /// <param name="worldGroup">WorldGroup the Relations should be updated on</param>
        /// <param name="relationId">Id of the Relation that should be updated</param>
        void UpdateRelation(string worldGroup, long relationId);

        /// <summary>
        /// Generates the Stats from all Servers 
        /// </summary>
        /// <returns>the actual result</returns>
        IEnumerable<string> RetrieveServerStatistics();

        /// <summary>
        /// Sends a Message to a specific Character
        /// </summary>
        /// <param name="worldGroup">WorldGroup that should be operated on</param>
        /// <param name="sourceCharacterId">CharacterId of the Sender</param>
        /// <param name="destinationCharacterId">CharacterId of the Receiver</param>
        /// <param name="messagePacket">The actual Message</param>
        /// <param name="sourceChannel">Channel we sent the Message from</param>
        /// <param name="messageType">Type of the Message</param>
        /// <returns></returns>
        int? SendMessageToCharacter(string worldGroup, long sourceCharacterId, long? destinationCharacterId, string messagePacket, int sourceChannel, MessageType messageType);

        /// <summary>
        /// Unregisters a previously registered World Server
        /// </summary>
        /// <param name="worldId">Id of the World to be unregistered</param>
        void UnregisterWorldServer(Guid worldId);
    }
}
