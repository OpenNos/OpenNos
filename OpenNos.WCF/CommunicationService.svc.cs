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

        private IDictionary<String, long> _registeredPlayerLogins;

        #endregion

        #region Properties

        public IDictionary<String, long> RegisteredPlayerLogins
        {
            get
            {
                if (_registeredPlayerLogins == null)
                {
                    _registeredPlayerLogins = new Dictionary<String, long>();
                }

                return _registeredPlayerLogins;
            }
        }

        #endregion

        #region Methods

        public bool HasRegisteredPlayerLogin(string name, long sessionId)
        {
            try {  //return if the player has been registered
                return RegisteredPlayerLogins.Remove(name);
            }
            catch(Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
            return false;
        }

        public void RegisterPlayerLogin(string name, long sessionId)
        {
            try { //TODO register player login
                if (!RegisteredPlayerLogins.ContainsKey(name))
                    RegisteredPlayerLogins.Add(name, sessionId);
                else
                {
                    RegisteredPlayerLogins.Remove(name);
                    RegisteredPlayerLogins.Add(name, sessionId);
                }

                ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                callback.RegisterPlayerLoginCallback(name);
            }
           catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        #endregion
    }
}
