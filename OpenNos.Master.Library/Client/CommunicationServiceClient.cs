using OpenNos.Core.Networking.Communication.ScsServices.Client;
using System;
using System.Collections.Generic;
using OpenNos.Master.Library.Interface;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Master.Library.Data;
using System.Configuration;

namespace OpenNos.Master.Library.Client
{
    public class CommunicationServiceClient : ICommunicationService
    {
        private static CommunicationServiceClient _instance;
        private IScsServiceClient<ICommunicationService> _client;

        public CommunicationServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["MasterIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
            _client = ScsServiceClientBuilder.CreateClient<ICommunicationService>(new ScsTcpEndPoint(ip, port));

            _client.Connect();
        }

        public static CommunicationServiceClient Instance => _instance ?? (_instance = new CommunicationServiceClient());

        public CommunicationStates CommunicationState
        {
            get
            {
                return _client.CommunicationState;
            }
        }

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

        public void RefreshPenalty(int penaltyId)
        {
            _client.ServiceProxy.RefreshPenalty(penaltyId);
        }

        public void RegisterAccountLogin(long accountId, long sessionId)
        {
            _client.ServiceProxy.RegisterAccountLogin(accountId, sessionId);
        }

        public int? RegisterWorldServer(WorldServer worldServer)
        {
            return _client.ServiceProxy.RegisterWorldServer(worldServer);
        }

        public IEnumerable<string> RetrieveServerStatistics()
        {
            return _client.ServiceProxy.RetrieveServerStatistics();
        }

        public int? SendMessageToCharacter(SCSCharacterMessage message)
        {
            return _client.ServiceProxy.SendMessageToCharacter(message);
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
    }
}
