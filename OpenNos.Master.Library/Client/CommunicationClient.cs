using System;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;

namespace OpenNos.Master.Library.Client
{
    internal class CommunicationClient : ICommunicationClient
    {
        #region Methods

        public void CharacterConnected(long characterId)
        {
            CommunicationServiceClient.Instance.OnCharacterConnected(characterId);
        }

        public void CharacterDisconnected(long characterId)
        {
            CommunicationServiceClient.Instance.OnCharacterDisconnected(characterId);
        }

        public void KickSession(long? accountId, long? sessionId)
        {
            CommunicationServiceClient.Instance.OnKickSession(accountId, sessionId);
        }

        public void SendMessageToCharacter(SCSCharacterMessage message)
        {
            CommunicationServiceClient.Instance.OnSendMessageToCharacter(message);
        }

        public void Shutdown()
        {
            CommunicationServiceClient.Instance.OnShutdown();
        }

        public void UpdateBazaar(long bazaarItemId)
        {
            CommunicationServiceClient.Instance.OnUpdateBazaar(bazaarItemId);
        }

        public void UpdateFamily(long familyId)
        {
            CommunicationServiceClient.Instance.OnUpdateFamily(familyId);
        }

        public void UpdatePenaltyLog(int penaltyLogId)
        {
            CommunicationServiceClient.Instance.OnUpdatePenaltyLog(penaltyLogId);
        }

        public void UpdateRelation(long relationId)
        {
            CommunicationServiceClient.Instance.OnUpdateRelation(relationId);
        }

        #endregion
    }
}