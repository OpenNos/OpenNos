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
        void RegisterAccountLogin(string name, long sessionId);

        [OperationContract]
        bool HasRegisteredAccountLogin(string name, long sessionId);

        [OperationContract]
        bool ConnectCharacter(string characterName);

        [OperationContract(IsOneWay = true)]
        void DisconnectCharacter(string characterName);
    }
}
