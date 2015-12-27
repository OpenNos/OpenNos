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
        void RegisterPlayerLogin(string name, long sessionId);

        [OperationContract]
        bool HasRegisteredPlayerLogin(string name, long sessionId);
    }
}
