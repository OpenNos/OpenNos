using System.ServiceModel;

namespace OpenNos.ServiceRef.Internal
{
    public class ServiceFactory
    {
        #region Members

        private CommunicationServiceReference.CommunicationServiceClient _communicationServiceClient;
        private CommunicationCallback _instanceCallback;
        private InstanceContext _instanceContext;

        #endregion

        #region Instantiation

        public ServiceFactory()
        {
            //callback instance will be instantiated once per process
            _instanceCallback = new CommunicationCallback();
            _instanceContext = new InstanceContext(_instanceCallback);
        }

        #region Singleton

        private static ServiceFactory _instance;

        public static ServiceFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceFactory();
                }

                return _instance;
            }
        }

        #endregion

        #endregion

        #region Properties

        public CommunicationServiceReference.CommunicationServiceClient CommunicationService
        {
            get
            {
                if (_communicationServiceClient == null || _communicationServiceClient.State == CommunicationState.Faulted)
                {

                    _communicationServiceClient = new CommunicationServiceReference.CommunicationServiceClient(_instanceContext);
                }

                return _communicationServiceClient;
            }
        }

        #endregion
    }
}
