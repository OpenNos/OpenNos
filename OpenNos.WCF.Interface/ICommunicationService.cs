using System.ServiceModel;

namespace OpenNos.WCF.Interface
{
    [ServiceContract(CallbackContract = typeof(ICommunicationCallback))]
    public interface ICommunicationService
    {
        #region Methods

        [OperationContract]
        bool AccountIsConnected(string accountName);

        [OperationContract(IsOneWay = true)]
        void Cleanup();

        [OperationContract]
        bool ConnectAccount(string accountName, int sessionId);

        [OperationContract]
        bool ConnectCharacter(string characterName, string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectAccount(string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectCharacter(string characterName);

        [OperationContract]
        bool HasRegisteredAccountLogin(string name, long sessionId);

        [OperationContract(IsOneWay = true)]
        void RegisterAccountLogin(string name, long sessionId);

        #endregion
    }
}