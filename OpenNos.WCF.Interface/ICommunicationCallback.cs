using System.ServiceModel;

namespace OpenNos.WCF.Interface
{
    public interface ICommunicationCallback
    {
        #region Methods

        [OperationContract(IsOneWay = true)]
        void ConnectAccountCallback(string accountName, int sessionId);

        [OperationContract(IsOneWay = true)]
        void ConnectCharacterCallback(string characterName);

        [OperationContract(IsOneWay = true)]
        void DisconnectAccountCallback(string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectCharacterCallback(string characterName);

        #endregion
    }
}