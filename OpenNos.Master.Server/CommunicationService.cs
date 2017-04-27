using OpenNos.Core.Networking.Communication.ScsServices.Service;
using OpenNos.Master.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Master.Library;

namespace OpenNos.Master.Server
{
    class CommunicationService : ScsService, ICommunicationService
    {
        public void Cleanup()
        {
            MSManager.Instance.ConnectedAccounts.Clear();
            MSManager.Instance.WorldServers.Clear();
        }

        public bool ConnectAccount(Guid worldId, long accountId, long sessionId)
        {
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a=>a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if(account!= null)
            {
                account.ConnectedWorld = MSManager.Instance.WorldServers.FirstOrDefault(w=>w.Id.Equals(worldId));
            }
            return account.ConnectedWorld == null ? false : true;
        }

        public bool ConnectCharacter(Guid worldId, long characterId)
        {
            throw new NotImplementedException();
        }

        public void DisconnectAccount(long accountId)
        {
            MSManager.Instance.ConnectedAccounts.RemoveAll(c => c.CharacterId.Equals(accountId));
        }

        public void DisconnectCharacter(Guid worldId, long characterId)
        {
            foreach (AccountConnection account in MSManager.Instance.ConnectedAccounts.Where(c => c.CharacterId.Equals(characterId) && c.ConnectedWorld.Equals(worldId)))
            {
                account.CharacterId = 0;
                account.ConnectedWorld = null;
            }
        }

        public bool IsAccountConnected(long accountId)
        {
            return MSManager.Instance.ConnectedAccounts.Any(c => c.AccountId == accountId && c.ConnectedWorld != null);
        }

        public bool IsLoginPermitted(long accountId, long sessionId)
        {
            return MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.ConnectedWorld == null);
        }

        public void KickSession(long? accountId, long? sessionId)
        {
            foreach (WorldServer world in MSManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationService>().KickSession(accountId, sessionId);
            }
            if (accountId.HasValue)
            {
                MSManager.Instance.ConnectedAccounts.RemoveAll(s => s.AccountId.Equals(accountId.Value));
            }
            else if (sessionId.HasValue)
            {
                MSManager.Instance.ConnectedAccounts.RemoveAll(s => s.SessionId.Equals(sessionId.Value));
            }
        }

        public void RefreshPenalty(int penaltyId)
        {
            foreach (WorldServer world in MSManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationService>().RefreshPenalty(penaltyId);
            }
            //TODO: Add Login Server refresh
        }

        public void RegisterAccountLogin(long accountId, long sessionId)
        {
            MSManager.Instance.ConnectedAccounts.Add(new AccountConnection(accountId, sessionId));
        }

        public int? RegisterWorldServer(WorldServer worldServer)
        {
            worldServer.ServiceClient = CurrentClient;
            worldServer.ChannelId = Enumerable.Range(1, 30).Except(MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldServer.WorldGroup)).OrderBy(w => w.ChannelId).Select(w => w.ChannelId)).First();
            MSManager.Instance.WorldServers.Add(worldServer);
            return worldServer.ChannelId;
        }

        public IEnumerable<string> RetrieveServerStatistics()
        {
            throw new NotImplementedException();
        }

        public int? SendMessageToCharacter(string worldGroup, long sourceCharacterId, long? destinationCharacterId, string messagePacket, int sourceChannel, OpenNos.Domain.MessageType messageType)
        {
            throw new NotImplementedException();
        }

        public void UnregisterWorldServer(Guid worldId)
        {
            MSManager.Instance.WorldServers.RemoveAll(w=>w.Id.Equals(worldId));
        }

        public void UpdateBazaar(string worldGroup, long bazaarItemId)
        {
            foreach(WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationService>().UpdateBazaar(worldGroup, bazaarItemId);
            }
        }

        public void UpdateFamily(string worldGroup, long familyId)
        {
            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationService>().UpdateFamily(worldGroup, familyId);
            }
        }

        public void UpdateRelation(string worldGroup, long relationId)
        {
            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationService>().UpdateRelation(worldGroup, relationId);
            }
        }
    }
}
