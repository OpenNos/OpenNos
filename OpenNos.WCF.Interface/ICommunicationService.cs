using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.WCF.Interface
{
    [ServiceContract(CallbackContract = typeof(ICommunicationCallback))]
    public interface ICommunicationService
    {
        [OperationContract(IsOneWay = true)]
        void Cleanup();

        [OperationContract(IsOneWay = true)]
        void RegisterAccountLogin(string name, long sessionId);

        [OperationContract]
        bool HasRegisteredAccountLogin(string name, long sessionId);

        [OperationContract]
        bool ConnectCharacter(string characterName, string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectCharacter(string characterName);

        [OperationContract]
        bool ConnectAccount(string accountName, int sessionId);

        [OperationContract]
        bool AccountIsConnected(string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectAccount(string accountName);       
    }
}
