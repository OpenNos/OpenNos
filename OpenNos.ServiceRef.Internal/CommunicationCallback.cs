using OpenNos.ServiceRef.Internal.CommunicationServiceReference;
using System;

namespace OpenNos.ServiceRef.Internal
{
    public class CommunicationCallback : ICommunicationServiceCallback, IDisposable
    {
        #region EventHandlers

        public EventHandler CharacterConnectedEvent;
        public EventHandler CharacterDisconnectedEvent;
        public EventHandler AccountConnectedEvent;
        public EventHandler AccountDisconnectedEvent;

        #endregion

        #region Methods

        public void ConnectCharacterCallback(string characterName)
        {
            //inform clients about a new connected character
            OnCharacterConnected(characterName);
        }

        public void DisconnectCharacterCallback(string characterName)
        {
            //inform clients about a disconnected character
            OnCharacterDisconnected(characterName);
        }

        public void ConnectAccountCallback(string accountName, int sessionId)
        {
            OnAccountConnected(accountName);
        }

        public void DisconnectAccountCallback(string accountName)
        {
            OnAccountDisconnected(accountName);
        }

        public void Dispose()
        {
            //dispose communication callback service
        }

        #region Private

        private void OnCharacterConnected(string characterName)
        {
            if (CharacterConnectedEvent != null && !String.IsNullOrEmpty(characterName))
            {
                CharacterConnectedEvent(characterName, new EventArgs());
            }
        }

        public void OnCharacterDisconnected(string characterName)
        {
            if (CharacterDisconnectedEvent != null && !String.IsNullOrEmpty(characterName))
            {
                CharacterDisconnectedEvent(characterName, new EventArgs());
            }
        }

        private void OnAccountConnected(string accountName)
        {
            if (AccountConnectedEvent != null && !String.IsNullOrEmpty(accountName))
            {
                AccountConnectedEvent(accountName, new EventArgs());
            }
        }

        public void OnAccountDisconnected(string accountName)
        {
            if (AccountDisconnectedEvent != null && !String.IsNullOrEmpty(accountName))
            {
                AccountDisconnectedEvent(accountName, new EventArgs());
            }
        }

        #endregion

        #endregion
    }
}
