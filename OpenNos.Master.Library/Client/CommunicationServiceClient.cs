using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Client;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace OpenNos.Master.Library.Client
{
    public class CommunicationServiceClient : ICommunicationService
    {
        #region Members

        private static CommunicationServiceClient _instance;
        private IScsServiceClient<ICommunicationService> _client;
        private CommunicationClient _commClient;

        #endregion

        #region Instantiation

        public CommunicationServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["MasterIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
            _commClient = new CommunicationClient();
            _client = ScsServiceClientBuilder.CreateClient<ICommunicationService>(new ScsTcpEndPoint(ip, port), _commClient);
            while (_client.CommunicationState != CommunicationStates.Connected)
            {
                try
                {
                    _client.Connect();
                }
                catch
                {
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("RETRY_CONNECTION"));
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler BazaarRefresh;

        public event EventHandler CharacterConnectedEvent;

        public event EventHandler CharacterDisconnectedEvent;

        public event EventHandler FamilyRefresh;

        public event EventHandler MessageSentToCharacter;

        public event EventHandler PenaltyLogRefresh;

        public event EventHandler RelationRefresh;

        public event EventHandler SessionKickedEvent;

        public event EventHandler ShutdownEvent;

        #endregion

        #region Properties

        public static CommunicationServiceClient Instance => _instance ?? (_instance = new CommunicationServiceClient());

        public CommunicationStates CommunicationState
        {
            get
            {
                return _client.CommunicationState;
            }
        }

        #endregion

        #region Methods

        public bool Authenticate(string authKey)
        {
            return _client.ServiceProxy.Authenticate(authKey);
        }

        public void Cleanup()
        {
            _client.ServiceProxy.Cleanup();
        }

        public bool ConnectAccount(Guid worldId, long accountId, long sessionId)
        {
            return _client.ServiceProxy.ConnectAccount(worldId, accountId, sessionId);
        }

        public bool ConnectCharacter(Guid worldId, long characterId)
        {
            return _client.ServiceProxy.ConnectCharacter(worldId, characterId);
        }

        public void DisconnectAccount(long accountId)
        {
            _client.ServiceProxy.DisconnectAccount(accountId);
        }

        public void DisconnectCharacter(Guid worldId, long characterId)
        {
            _client.ServiceProxy.DisconnectCharacter(worldId, characterId);
        }

        public int? GetChannelIdByWorldId(Guid worldId)
        {
            return _client.ServiceProxy.GetChannelIdByWorldId(worldId);
        }

        public bool IsAccountConnected(long accountId)
        {
            return _client.ServiceProxy.IsAccountConnected(accountId);
        }

        public bool IsCharacterConnected(string worldGroup, long characterId)
        {
            return _client.ServiceProxy.IsCharacterConnected(worldGroup, characterId);
        }

        public bool IsLoginPermitted(long accountId, long sessionId)
        {
            return _client.ServiceProxy.IsLoginPermitted(accountId, sessionId);
        }

        public void KickSession(long? accountId, long? sessionId)
        {
            _client.ServiceProxy.KickSession(accountId, sessionId);
        }

        public void PulseAccount(long accountId)
        {
            _client.ServiceProxy.PulseAccount(accountId);
        }

        public void RefreshPenalty(int penaltyId)
        {
            _client.ServiceProxy.RefreshPenalty(penaltyId);
        }

        public void RegisterAccountLogin(long accountId, long sessionId)
        {
            _client.ServiceProxy.RegisterAccountLogin(accountId, sessionId);
        }

        public int? RegisterWorldServer(SerializableWorldServer worldServer)
        {
            return _client.ServiceProxy.RegisterWorldServer(worldServer);
        }

        public string RetrieveRegisteredWorldServers(long sessionId)
        {
            return _client.ServiceProxy.RetrieveRegisteredWorldServers(sessionId);
        }

        public IEnumerable<string> RetrieveServerStatistics()
        {
            return _client.ServiceProxy.RetrieveServerStatistics();
        }

        public int? SendMessageToCharacter(SCSCharacterMessage message)
        {
            return _client.ServiceProxy.SendMessageToCharacter(message);
        }

        public void Shutdown(string worldGroup)
        {
            _client.ServiceProxy.Shutdown(worldGroup);
        }

        public void UnregisterWorldServer(Guid worldId)
        {
            _client.ServiceProxy.UnregisterWorldServer(worldId);
        }

        public void UpdateBazaar(string worldGroup, long bazaarItemId)
        {
            _client.ServiceProxy.UpdateBazaar(worldGroup, bazaarItemId);
        }

        public void UpdateFamily(string worldGroup, long familyId)
        {
            _client.ServiceProxy.UpdateFamily(worldGroup, familyId);
        }

        public void UpdateRelation(string worldGroup, long relationId)
        {
            _client.ServiceProxy.UpdateRelation(worldGroup, relationId);
        }

        internal void OnCharacterConnected(long characterId)
        {
            string characterName = DAOFactory.CharacterDAO.LoadById(characterId)?.Name;
            CharacterConnectedEvent?.Invoke(new Tuple<long, string>(characterId, characterName), null);
        }

        internal void OnCharacterDisconnected(long characterId)
        {
            string characterName = DAOFactory.CharacterDAO.LoadById(characterId)?.Name;
            CharacterDisconnectedEvent?.Invoke(new Tuple<long, string>(characterId, characterName), null);
        }

        internal void OnKickSession(long? accountId, long? sessionId)
        {
            SessionKickedEvent?.Invoke(new Tuple<long?, long?>(accountId, sessionId), null);
        }

        internal void OnSendMessageToCharacter(SCSCharacterMessage message)
        {
            MessageSentToCharacter?.Invoke(message, null);
        }

        internal void OnShutdown()
        {
            ShutdownEvent?.Invoke(null, null);
        }

        internal void OnUpdateBazaar(long bazaarItemId)
        {
            BazaarRefresh?.Invoke(bazaarItemId, null);
        }

        internal void OnUpdateFamily(long familyId)
        {
            FamilyRefresh?.Invoke(familyId, null);
        }

        internal void OnUpdatePenaltyLog(int penaltyLogId)
        {
            PenaltyLogRefresh?.Invoke(penaltyLogId, null);
        }

        internal void OnUpdateRelation(long relationId)
        {
            RelationRefresh?.Invoke(relationId, null);
        }

        #endregion
    }
}