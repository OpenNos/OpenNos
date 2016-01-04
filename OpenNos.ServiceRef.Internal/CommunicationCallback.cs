using OpenNos.ServiceRef.Internal.CommunicationServiceReference;
using System;

namespace OpenNos.ServiceRef.Internal
{
    public class CommunicationCallback : ICommunicationServiceCallback, IDisposable
    {
        #region EventHandler

        public EventHandler CharacterConnectedEvent;

        public EventHandler CharacterDisconnectedEvent;

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

        #endregion

        #endregion
    }
}
