using log4net;
using OpenNos.Core;
using OpenNos.WCF.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace OpenNos.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CommunicationService : ICommunicationService
    {
        #region Members

        private IDictionary<String, long> _registeredAccountLogins;
        private IList<String> _connectedCharacters;

        #endregion

        #region Instantiation


        public CommunicationService()
        {
            Logger.InitializeLogger(LogManager.GetLogger(typeof(CommunicationService)));
        }
        #endregion

        #region Properties

        public IDictionary<String, long> RegisteredAccountLogins
        {
            get
            {
                if (_registeredAccountLogins == null)
                {
                    _registeredAccountLogins = new Dictionary<String, long>();
                }

                return _registeredAccountLogins;
            }
        }

        public IList<String> ConnectedCharacters
        {
            get
            {
                if (_connectedCharacters == null)
                {
                    _connectedCharacters = new List<String>();
                }

                return _connectedCharacters;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the Account is allowed to login, removes the permission to login
        /// </summary>
        /// <param name="accountName">Name of the Account.</param>
        /// <param name="sessionId">SessionId to check for validity.</param>
        /// <returns></returns>
        public bool HasRegisteredAccountLogin(string accountName, long sessionId)
        {
            try
            {
                //return if the player has been registered
                bool successful = RegisteredAccountLogins.Remove(accountName);

                if (successful)
                {
                    Logger.Log.DebugFormat("[WCF] Account {0} has lost the permission to login with SessionId {1}.", accountName, sessionId);
                }
                else
                {
                    Logger.Log.DebugFormat("[WCF] Account {0} is not permitted to loginw with SessionId {1}.", accountName, sessionId);
                }

                return successful;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }

            return false;
        }
        
        /// <summary>
        /// Register Account for Login (Verification for Security)
        /// </summary>
        /// <param name="accountName">Name of the Account</param>
        /// <param name="sessionId">SessionId for the valid connection.</param>
        public void RegisterAccountLogin(string accountName, long sessionId)
        {
            try
            {
                if (!RegisteredAccountLogins.ContainsKey(accountName))
                    RegisteredAccountLogins.Add(accountName, sessionId);
                else
                {
                    RegisteredAccountLogins.Remove(accountName);
                    RegisteredAccountLogins.Add(accountName, sessionId);
                }

                Logger.Log.DebugFormat("[WCF] Account {0} is now permitted to login with SessionId {1}", accountName, sessionId);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers that the given Character has now logged in
        /// </summary>
        /// <param name="characterName">Name of the Character.</param>
        public bool ConnectCharacter(string characterName)
        {
            try
            {
                //character cant connect twice
                if (ConnectedCharacters.Contains(characterName))
                {
                    Logger.Log.DebugFormat("[WCF] Character {0} is already connected.", characterName);
                    return false;
                }
                else
                {
                    Logger.Log.DebugFormat("[WCF] Character {0} has connected.", characterName);
                    ConnectedCharacters.Add(characterName);

                    //inform clients
                    ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                    callback.ConnectCharacterCallback(characterName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Disconnect character from server.
        /// </summary>
        /// <param name="characterName">Character who wants to disconnect.</param>
        public void DisconnectCharacter(string characterName)
        {
            try
            {
                ConnectedCharacters.Remove(characterName);

                //inform clients
                ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                callback.DisconnectCharacterCallback(characterName);

                Logger.Log.DebugFormat("[WCF] Character {0} has been disconnected.", characterName);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        #endregion
    }
}
